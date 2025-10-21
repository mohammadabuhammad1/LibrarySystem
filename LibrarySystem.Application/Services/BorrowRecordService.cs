using AutoMapper;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace LibrarySystem.Application.Services;

public class BorrowRecordService(
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    IMapper mapper) : IBorrowRecordService
{
    private const decimal FINE_PER_DAY = 0.50m;
    private const int MAX_RENEWAL_DAYS = 30;
    private const int MAX_RENEWAL_COUNT = 3;
    private const int MAX_BORROW_LIMIT = 5;

    public async Task<BorrowRecordDto> BorrowBookAsync(CreateBorrowRecordDto borrowDto)
    {
        ArgumentNullException.ThrowIfNull(borrowDto);

        if (string.IsNullOrWhiteSpace(borrowDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(borrowDto));

        ApplicationUser? user = await userManager
            .FindByIdAsync(borrowDto.UserId)
            .ConfigureAwait(false);

        if (user == null)
            throw new InvalidOperationException($"User with ID {borrowDto.UserId} not found.");

        if (!user.IsActive)
            throw new InvalidOperationException($"User {user.Name} is not active.");

        Book? book = await unitOfWork.Books
            .GetByIdTrackedAsync(borrowDto.BookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {borrowDto.BookId} not found.");

        if (!book.CanBorrow())
            throw new InvalidOperationException($"No copies available for '{book.Title}'.");

        // Rule Enforcement: Check for existing active borrow
        BorrowRecord? existingBorrow = await unitOfWork.BorrowRecords
            .GetActiveBorrowByBookAndUserAsync(borrowDto.BookId, borrowDto.UserId)
            .ConfigureAwait(false);

        if (existingBorrow != null)
            throw new InvalidOperationException($"User already has '{book.Title}' borrowed.");

        // Rule Enforcement: Check for Overdue Books and Borrow Limit
        IEnumerable<BorrowRecord> activeBorrows = await unitOfWork.BorrowRecords
            .GetActiveBorrowsByUserAsync(borrowDto.UserId)
            .ConfigureAwait(false);

        if (activeBorrows.Any(b => b.IsOverdue()))
            throw new InvalidOperationException("User has one or more overdue books and cannot borrow until they are returned.");

        if (activeBorrows.Count() >= MAX_BORROW_LIMIT)
            throw new InvalidOperationException($"User has reached the maximum borrowing limit of {MAX_BORROW_LIMIT} books.");

        BorrowRecord borrowRecord = BorrowRecord.Create(
            borrowDto.BookId,
            borrowDto.UserId,
            borrowDto.BorrowDurationDays,
            borrowDto.Notes);

        BorrowRecord? createdRecord = await unitOfWork.BorrowRecords
            .AddAsync(borrowRecord)
            .ConfigureAwait(false);

        book.Borrow();

        // NOTE: unitOfWork.Books.UpdateAsync(book) is redundant if 'book' is tracked,
        // but often kept for clarity/safety. We'll leave it in your current structure.
        await unitOfWork.Books
            .UpdateAsync(book)
            .ConfigureAwait(false);

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to create borrow record.");

        createdRecord = await unitOfWork.BorrowRecords
            .GetBorrowRecordWithDetailsAsync(createdRecord.Id)
            .ConfigureAwait(false);

        return mapper.Map<BorrowRecordDto>(createdRecord);
    }

    public async Task<BorrowRecordDto> ReturnBookAsync(ReturnBookDto returnDto)
    {
        ArgumentNullException.ThrowIfNull(returnDto);

        if (string.IsNullOrWhiteSpace(returnDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty", returnDto.UserId);

        BorrowRecord? borrowRecord = await unitOfWork.BorrowRecords
            .GetActiveBorrowByBookAndUserAsync(returnDto.BookId, returnDto.UserId)
            .ConfigureAwait(false);

        if (borrowRecord == null)
            throw new InvalidOperationException($"No active borrow record found for Book {returnDto.BookId} and User {returnDto.UserId}");

        decimal? fineAmount = returnDto.FineAmount;
        if (fineAmount == null)
        {
            fineAmount = borrowRecord.CalculateFine(FINE_PER_DAY);
        }

        borrowRecord.ReturnBook(fineAmount, returnDto.Notes, returnDto.Condition);

        // NOTE: UpdateAsync is often redundant for tracked entities but kept here for consistency
        await unitOfWork.BorrowRecords
            .UpdateAsync(borrowRecord)
            .ConfigureAwait(false);

        // 2. Retrieve Book as a TRACKED entity for modification (book.Return())
        Book? book = await unitOfWork.Books
            .GetByIdTrackedAsync(returnDto.BookId)
            .ConfigureAwait(false);

        if (book != null)
        {
            book.Return();
            await unitOfWork.Books
                .UpdateAsync(book)
                .ConfigureAwait(false);
        }

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to return book.");

        borrowRecord = await unitOfWork.BorrowRecords
            .GetBorrowRecordWithDetailsAsync(borrowRecord.Id)
            .ConfigureAwait(false);

        return mapper.Map<BorrowRecordDto>(borrowRecord);
    }

    public async Task<IEnumerable<BorrowRecordDto>> GetUserBorrowHistoryAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        IEnumerable<BorrowRecord> records = await unitOfWork.BorrowRecords
            .GetBorrowHistoryByUserAsync(userId)
            .ConfigureAwait(false);

        return mapper.Map<IEnumerable<BorrowRecordDto>>(records);
    }

    public async Task<IEnumerable<BorrowRecordDto>> GetOverdueBooksAsync()
    {
        IEnumerable<BorrowRecord> overdueRecords = await unitOfWork.BorrowRecords.
            GetOverdueBorrowsAsync()
            .ConfigureAwait(false);

        return mapper.Map<IEnumerable<BorrowRecordDto>>(overdueRecords);
    }

    public async Task<IEnumerable<BorrowRecordDto>> GetActiveBorrowsByUserAsync(string userId)
    {
        IEnumerable<BorrowRecord> activeBorrows = await unitOfWork.BorrowRecords
            .GetActiveBorrowsByUserAsync(userId)
            .ConfigureAwait(false);

        return mapper.Map<IEnumerable<BorrowRecordDto>>(activeBorrows);
    }

    public async Task<decimal> CalculateFineAsync(int borrowRecordId)
    {
        BorrowRecord? record = await unitOfWork.BorrowRecords
            .GetByIdAsync(borrowRecordId)
            .ConfigureAwait(false);

        if (record == null)
            throw new InvalidOperationException($"Borrow record with ID {borrowRecordId} not found.");

        return record.CalculateFine(FINE_PER_DAY);
    }

    public async Task<bool> CanUserViewFineAsync(int borrowRecordId, string userId)
    {
        IEnumerable<BorrowRecord> userBorrowHistory = await unitOfWork.BorrowRecords
            .GetBorrowHistoryByUserAsync(userId)
            .ConfigureAwait(false);

        return userBorrowHistory.Any(br => br.Id == borrowRecordId);
    }

    public async Task<IEnumerable<BorrowRecordDto>> GetBorrowHistoryByBookAsync(int bookId)
    {
        Book? book = await unitOfWork.Books
            .GetByIdAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found.");

        IEnumerable<BorrowRecord> records = await unitOfWork.BorrowRecords
            .GetBorrowHistoryByBookAsync(bookId)
            .ConfigureAwait(false);

        return mapper.Map<IEnumerable<BorrowRecordDto>>(records);
    }

    public async Task<BorrowRecordDto> RenewBorrowAsync(int borrowRecordId, int additionalDays, string userId)
    {
        if (additionalDays <= 0)
            throw new InvalidOperationException("Additional days must be greater than zero.");

        if (additionalDays > MAX_RENEWAL_DAYS)
            throw new InvalidOperationException($"Maximum renewal period is {MAX_RENEWAL_DAYS} days.");

        BorrowRecord? borrowRecord = await unitOfWork.BorrowRecords
            .GetBorrowRecordWithDetailsAsync(borrowRecordId)
            .ConfigureAwait(false);

        if (borrowRecord == null)
            throw new InvalidOperationException($"Borrow record with ID {borrowRecordId} not found.");

        if (borrowRecord.UserId != userId)
            throw new InvalidOperationException("You can only renew your own borrow records.");

        if (borrowRecord.IsReturned)
            throw new InvalidOperationException("Cannot renew a book that has already been returned.");

        if (borrowRecord.IsOverdue())
            throw new InvalidOperationException("Cannot renew an overdue book. Please return it and pay any fines first.");

        Book? book = borrowRecord.Book;
        if (book == null)
            throw new InvalidOperationException("Book information not found.");

        if (!book.CanBorrow())
            throw new InvalidOperationException("Cannot renew book as all copies are currently borrowed.");

        if (!borrowRecord.CanRenew(MAX_RENEWAL_COUNT))
            throw new InvalidOperationException($"Maximum renewal count ({MAX_RENEWAL_COUNT}) reached for this book.");

        DateTime newDueDate = borrowRecord.DueDate.AddDays(additionalDays);
        string renewalNotes = $"Renewed on {DateTime.UtcNow:yyyy-MM-dd}, new due date: {newDueDate:yyyy-MM-dd}";
        borrowRecord.Renew(newDueDate, renewalNotes);

        await unitOfWork.BorrowRecords
            .UpdateAsync(borrowRecord)
            .ConfigureAwait(false);

        bool success = await unitOfWork.CommitAsync().ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to renew borrow record.");

        return mapper.Map<BorrowRecordDto>(borrowRecord);
    }

    public async Task<IEnumerable<BookDto>> GetBorrowedBooksByUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        IEnumerable<BorrowRecord> activeBorrows = await unitOfWork.BorrowRecords
            .GetActiveBorrowsByUserAsync(userId)
            .ConfigureAwait(false);

        // Map the Book property of each BorrowRecord to a BookDto
        // We assume the repository method eager-loaded the Book navigation property.
        return mapper.Map<IEnumerable<BookDto>>(activeBorrows
            .Where(b => b.Book != null)
            .Select(b => b.Book!));
    }

    public async Task<bool> CanUserBorrowAsync(string userId)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user == null || !user.IsActive)
            return false;

        IEnumerable<BorrowRecord> activeBorrows = await unitOfWork.BorrowRecords
            .GetActiveBorrowsByUserAsync(userId)
            .ConfigureAwait(false);

        if (activeBorrows.Any(b => b.IsOverdue()))
            return false;

        if (activeBorrows.Count() >= MAX_BORROW_LIMIT)
            return false;

        return true;
    }

    public async Task<BorrowRecordDto?> GetActiveBorrowByBookAsync(int bookId)
    {
        BorrowRecord? activeBorrow = await unitOfWork.BorrowRecords
            .GetActiveBorrowByBookAsync(bookId)
            .ConfigureAwait(false);

        if (activeBorrow == null)
            return null;

        return mapper.Map<BorrowRecordDto>(activeBorrow);
    }
}
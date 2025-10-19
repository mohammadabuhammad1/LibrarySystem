using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Domain.Entities;

public class BorrowRecord : BaseEntity
{
    public int BookId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public DateTime BorrowDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? ReturnDate { get; private set; }
    public bool IsReturned { get; private set; }
    public decimal? FineAmount { get; private set; }
    public string? Notes { get; private set; }
    public BookCondition Condition { get; private set; } = BookCondition.Good;
    public int RenewalCount { get; private set; }

    public Book? Book { get;  set; }
    public ApplicationUser? User { get;  set; }

    internal BorrowRecord() { }

    public static BorrowRecord Create(int bookId, string userId, int borrowDurationDays, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is required", nameof(userId));

        if (borrowDurationDays <= 0)
            throw new ArgumentException("Borrow duration must be greater than 0", nameof(borrowDurationDays));

        var borrowRecord = new BorrowRecord
        {
            BookId = bookId,
            UserId = userId,
            BorrowDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(borrowDurationDays),
            IsReturned = false,
            Notes = notes,
            Condition = BookCondition.Good,
            RenewalCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        return borrowRecord;
    }

    public void ReturnBook(decimal? fineAmount = null, string? notes = null, BookCondition condition = BookCondition.Good)
    {
        if (IsReturned)
            throw new InvalidOperationException("Book is already returned");

        ReturnDate = DateTime.UtcNow;
        IsReturned = true;
        FineAmount = fineAmount;
        Condition = condition;

        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes += $"\nReturn: {notes}";
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsReturned(string? notes = null)
    {
        if (IsReturned)
            throw new InvalidOperationException("Book already returned");

        ReturnDate = DateTime.UtcNow;
        IsReturned = true;

        if (!string.IsNullOrEmpty(notes))
            Notes = string.IsNullOrEmpty(Notes) ? notes : $"{Notes} | {notes}";

        UpdatedAt = DateTime.UtcNow;
    }

    public void Renew(DateTime newDueDate, string renewalNotes)
    {
        if (IsReturned)
            throw new InvalidOperationException("Cannot renew a returned book");

        if (newDueDate <= DueDate)
            throw new InvalidOperationException("New due date must be after current due date");

        DueDate = newDueDate;
        RenewalCount++;

        var existingNotes = Notes ?? string.Empty;
        Notes = string.IsNullOrEmpty(existingNotes)
            ? renewalNotes
            : $"{existingNotes} | {renewalNotes}";

        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanRenew(int maxRenewalCount)
    {
        return !IsReturned && RenewalCount < maxRenewalCount;
    }

    public void ApplyFine(decimal amount, string? reason = null)
    {
        if (amount < 0)
            throw new ArgumentException("Fine amount cannot be negative", nameof(amount));

        FineAmount = amount;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes += $"\nFine applied: {reason} - ${amount}";
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string additionalNotes)
    {
        if (string.IsNullOrWhiteSpace(additionalNotes))
            return;

        Notes = string.IsNullOrEmpty(Notes)
            ? additionalNotes
            : $"{Notes} | {additionalNotes}";

        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOverdue() => !IsReturned && DateTime.UtcNow > DueDate;

    public int DaysOverdue() => IsOverdue() ? (DateTime.UtcNow - DueDate).Days : 0;

    public decimal CalculateFine(decimal finePerDay)
    {
        if (IsReturned && FineAmount.HasValue)
            return FineAmount.Value;

        if (IsOverdue())
            return DaysOverdue() * finePerDay;

        return 0;
    }

    public string ConditionDescription => Condition switch
    {
        BookCondition.Excellent => "Like new",
        BookCondition.Good => "Minor wear",
        BookCondition.Fair => "Noticeable wear",
        BookCondition.Poor => "Significant damage",
        BookCondition.Damaged => "Major damage",
        BookCondition.Lost => "Lost",
        _ => "Unknown"
    };
}
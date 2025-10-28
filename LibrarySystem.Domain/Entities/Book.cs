using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Common.DomainEvents;
using LibrarySystem.Domain.ValueObjects;

namespace LibrarySystem.Domain.Entities;

public class Book : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string ISBN { get; private set; } = string.Empty;
    public int PublishedYear { get; private set; }
    public int TotalCopies { get; private set; }
    public int CopiesAvailable { get; private set; }
    public string? Description { get; private set; }
    public string? Genre { get; private set; }

    public bool IsAvailable => CopiesAvailable > 0;
    public int BorrowedCopiesCount => TotalCopies - CopiesAvailable;
    public bool IsOutOfStock => CopiesAvailable == 0;
    public bool HasAvailableCopies => CopiesAvailable > 0;
    public decimal UtilizationRate => TotalCopies > 0 ? (decimal)BorrowedCopiesCount / TotalCopies : 0;

    public int? LibraryId { get; private set; }
    public Library? Library { get; }

    public ICollection<BorrowRecord> BorrowRecords { get; } = [];

    private Book() { }

    public static Book Create(string title, string author, string isbn, int publishedYear,
        int totalCopies, string? description = null, string? genre = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author is required", nameof(author));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN is required", nameof(isbn));

        if (totalCopies <= 0)
            throw new ArgumentException("Total copies must be greater than 0", nameof(totalCopies));

        Isbn validatedIsbn = Isbn.Create(isbn);

        var book = new Book
        {
            Title = title.Trim(),
            Author = author.Trim(),
            ISBN = validatedIsbn.Value, 
            PublishedYear = publishedYear,
            TotalCopies = totalCopies,
            CopiesAvailable = totalCopies,
            Description = description,
            Genre = genre,
            CreatedAt = DateTime.UtcNow
        };

        book.AddDomainEvent(new BookCreatedEvent(book.Id, book.Title, book.ISBN, book.Description , book.Genre));

        return book;
    }

    public void Update(
    string? title,          
    string? author,         
    int? publishedYear,     
    int? totalCopies,       
    string? description,    
    string? genre)          
    {


        // Update Title if provided (not null/empty)
        if (!string.IsNullOrWhiteSpace(title))
        {
            this.Title = title.Trim();
        }

        // Update Author if provided (not null/empty)
        if (!string.IsNullOrWhiteSpace(author))
        {
            this.Author = author.Trim();
        }

        // Update PublishedYear if provided (HasValue)
        if (publishedYear.HasValue)
        {
            var newYear = publishedYear.Value;
            if (newYear <= 0 || newYear > DateTime.UtcNow.Year + 1)
                throw new ArgumentException("Published year must be valid", nameof(publishedYear));

            PublishedYear = newYear;
        }


        if (description != null)
        {
            Description = description.Trim();
        }

        if (genre != null)
        {
            Genre = genre.Trim();
        }

        if (totalCopies.HasValue)
        {
            var newTotalCopies = totalCopies.Value;

            if (newTotalCopies < 0)
                throw new ArgumentException("Total copies cannot be negative", nameof(totalCopies));

            if (newTotalCopies < (TotalCopies - CopiesAvailable))
                throw new InvalidOperationException("New total copies cannot be less than the number of currently borrowed copies.");

            if (newTotalCopies != TotalCopies)
            {
                var diff = newTotalCopies - TotalCopies;
                TotalCopies = newTotalCopies;
                CopiesAvailable = Math.Max(0, CopiesAvailable + diff);
            }
        }

        UpdatedAt = DateTime.UtcNow;

    }

    public bool CanBorrow()
    {
        return CopiesAvailable > 0;
    }
    public void Borrow()
    {
        if (!CanBorrow())
            throw new InvalidOperationException("No copies available for borrowing");

        CopiesAvailable--;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Return()
    {
        if (CopiesAvailable >= TotalCopies)
            throw new InvalidOperationException($"'{Title}' is not currently borrowed.");
        CopiesAvailable++;
        UpdatedAt = DateTime.UtcNow;
    }
    public void MarkAsDamaged()
    {
        if (TotalCopies <= 0)
            throw new InvalidOperationException("No copies to mark as damaged");

        TotalCopies--;
        CopiesAvailable = Math.Max(0, CopiesAvailable - 1);
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateCopies(int totalCopies)
    {
        if (totalCopies < 0)
            throw new ArgumentException("Total copies cannot be negative", nameof(totalCopies));

        var diff = totalCopies - TotalCopies;
        TotalCopies = totalCopies;
        CopiesAvailable = Math.Max(0, CopiesAvailable + diff);
        UpdatedAt = DateTime.UtcNow;
    }
    public void Restock(int additionalCopies)
    {
        if (additionalCopies <= 0)
            throw new ArgumentException("Additional copies must be greater than 0", nameof(additionalCopies));

        TotalCopies += additionalCopies;
        CopiesAvailable += additionalCopies;
        UpdatedAt = DateTime.UtcNow;
    }
    public void AssignToLibrary(int libraryId)
    {
        if (libraryId <= 0)
            throw new ArgumentException("Library ID must be greater than 0", nameof(libraryId));

        LibraryId = libraryId;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateDetails(string title, string author, string isbn, int publishedYear, int totalCopies)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be empty.", nameof(author));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN cannot be empty.", nameof(isbn));

        if (publishedYear <= 0)
            throw new ArgumentException("Published year must be a positive number.", nameof(publishedYear));

        if (totalCopies < 0)
            throw new ArgumentException("Total copies cannot be negative.", nameof(totalCopies));

        Isbn validatedIsbn = Isbn.Create(isbn);

        Title = title;
        Author = author;
        ISBN = validatedIsbn.Value;
        PublishedYear = publishedYear;
        TotalCopies = totalCopies;

        if (CopiesAvailable > TotalCopies)
            CopiesAvailable = TotalCopies;
    }

}
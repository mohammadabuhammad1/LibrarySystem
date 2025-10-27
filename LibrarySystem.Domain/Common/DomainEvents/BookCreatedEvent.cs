using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.Common.DomainEvents;

public class BookCreatedEvent(int bookId, string title, string isbn , string? description , string? genre) : IDomainEvent
{
    public int BookId { get; } = bookId;
    public string Title { get; } = title;
    public string ISBN { get; } = isbn;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string? Description { get; set; } = description;
    public string? Genre { get; set; } = genre;
}
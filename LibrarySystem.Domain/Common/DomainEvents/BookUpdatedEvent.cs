using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.Common.DomainEvents;
public class BookUpdatedEvent(int bookId, string title) : IDomainEvent
{
    public int BookId { get; } = bookId;
    public string Title { get; } = title;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
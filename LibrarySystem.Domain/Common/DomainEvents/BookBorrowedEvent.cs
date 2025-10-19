using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.Common.DomainEvents;

public class BookBorrowedEvent(int bookId, string userId, DateTime dueDate) : IDomainEvent
{
    public int BookId { get; } = bookId;
    public string UserId { get; } = userId;
    public DateTime DueDate { get; } = dueDate;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
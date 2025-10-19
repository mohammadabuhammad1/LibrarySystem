using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.Commands.Books;

public class BorrowBookCommand : BaseCommand
{
    public int BookId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int BorrowDurationDays { get; set; } = 14;
    public string? Notes { get; set; }
}
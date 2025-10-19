using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Domain.Commands.Books;

public class ReturnBookCommand : BaseCommand
{
    public int BookId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public BookCondition Condition { get; set; } = BookCondition.Good; 
    public decimal? FineAmount { get; set; }
}
using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Application.Dtos.Books;
public class ReturnBookDto
{
    public string UserId { get; set; } = string.Empty; 
    public string? Notes { get; set; }
    public BookCondition Condition { get; set; } = BookCondition.Good;
    public decimal? FineAmount { get; set; }
}
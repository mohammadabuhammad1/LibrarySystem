using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Application.Dtos.Books;

public class BorrowRecordDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned { get; set; }
    public decimal? FineAmount { get; set; }
    public string? Notes { get; set; }
    public BookCondition Condition { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysOverdue { get; set; }
    public int RenewalCount { get; set; } 
    public string ConditionDescription { get; set; } = string.Empty;
}
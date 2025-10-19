namespace LibrarySystem.Application.Dtos.Books;
public class BookStatsDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int CopiesAvailable { get; set; }
    public int BorrowedCopiesCount { get; set; }
    public decimal UtilizationRate { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsOutOfStock { get; set; }
}
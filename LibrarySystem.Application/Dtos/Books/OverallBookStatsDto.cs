namespace LibrarySystem.Application.Dtos.Books;

public class OverallBookStatsDto
{
    public int TotalBooks { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public int BorrowedCopies { get; set; }
    public int TotalLibraries { get; set; }
    public int DamagedCopies { get; set; }
    public decimal UtilizationRate { get; set; }
    public int OutOfStockBooks { get; set; }
    public int AvailableBooks { get; set; }


    public decimal AvailabilityRate => TotalCopies > 0 ? (decimal)AvailableCopies / TotalCopies * 100 : 0;
    public decimal BorrowRate => TotalCopies > 0 ? (decimal)BorrowedCopies / TotalCopies * 100 : 0;
}
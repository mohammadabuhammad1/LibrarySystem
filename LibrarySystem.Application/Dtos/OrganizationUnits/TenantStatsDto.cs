namespace LibrarySystem.Application.Dtos.OrganizationUnits;
public class TenantStatsDto
{
    public int TotalLibraries { get; set; }
    public int TotalUsers { get; set; }
    public int TotalBooks { get; set; }
    public int TotalBorrowedBooks { get; set; }
    public int ActiveBorrows { get; set; }
    public decimal TotalFines { get; set; }
    public DateTime? LastActivity { get; set; }
}
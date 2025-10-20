

namespace LibrarySystem.Application.Dtos.OrganizationUnits;
public class TenantDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public int? MaxLibraries { get; set; }
    public int? MaxUsers { get; set; }
    public int LibrariesCount { get; set; }
    public int UsersCount { get; set; }
    public bool HasSeparateDatabase { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

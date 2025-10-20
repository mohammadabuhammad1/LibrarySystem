using System.ComponentModel.DataAnnotations;


namespace LibrarySystem.Application.Dtos.OrganizationUnits;
public class UpdateTenantDto
{
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public int? MaxLibraries { get; set; }
    public int? MaxUsers { get; set; }
    public bool IsActive { get; set; }
}


using LibrarySystem.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Domain.Entities;
public class Tenant : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    [EmailAddress]
    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }

    public int? MaxLibraries { get; set; }
    public int? MaxUsers { get; set; }

    public bool UseSeparateDatabase { get; set; }
    public string? ConnectionString { get; set; }

    // Foreign key to OrganizationUnit
    public int OrganizationUnitId { get; set; }

    // Navigation property
    [ForeignKey("OrganizationUnitId")]
    public virtual OrganizationUnit OrganizationUnit { get; set; } = null!;

    // Navigation properties - initialize with empty collection
    public ICollection<TenantFeature> Features { get; } = [];

    public Tenant()
    {
        CreatedAt = DateTime.UtcNow;
    }
}

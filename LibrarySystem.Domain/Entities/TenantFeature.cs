using LibrarySystem.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Domain.Entities;
public class TenantFeature : BaseEntity
{
    [Required]
    public int TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Value { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    public TenantFeature()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
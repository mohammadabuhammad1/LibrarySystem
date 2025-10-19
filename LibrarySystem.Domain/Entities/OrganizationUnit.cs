using LibrarySystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Domain.Entities;

/// <summary>
/// Organization Unit for hierarchical multi-tenancy
/// Supports structure like: 0001.0001.0001
/// </summary>
public class OrganizationUnit
{
    public int Id { get; set; }

    /// <summary>
    /// Unique hierarchical code (e.g., "0001.0001.0001")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the organization unit
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Optional description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Parent Organization Unit ID (null for root)
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Navigation property to parent
    /// </summary>
    public virtual OrganizationUnit? Parent { get; set; }

    /// <summary>
    /// Child organization units
    /// </summary>
    public virtual ICollection<OrganizationUnit> Children { get;private set; } = [];

    /// <summary>
    /// Libraries belonging to this OU
    /// </summary>
    public virtual ICollection<Library> Libraries { get; private set; } = [];

    /// <summary>
    /// Users belonging to this OU
    /// </summary>
    public virtual ICollection<UserOrganizationUnit> UserOrganizationUnits { get; private set; } = []   ;

    /// <summary>
    /// Is this OU active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Organization type (e.g., Tenant, Branch, Department)
    /// </summary>
    [MaxLength(50)]
    public string Type { get; set; } = "Branch";

    /// <summary>
    /// Contact email for this organization
    /// </summary>
    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Contact phone
    /// </summary>
    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Subscription start date (for tenants)
    /// </summary>
    public DateTime? SubscriptionStartDate { get; set; }

    /// <summary>
    /// Subscription end date (for tenants)
    /// </summary>
    public DateTime? SubscriptionEndDate { get; set; }

    /// <summary>
    /// Maximum number of libraries allowed
    /// </summary>
    public int? MaxLibraries { get; set; }

    /// <summary>
    /// Maximum number of users allowed
    /// </summary>
    public int? MaxUsers { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Helper methods
    public bool IsRoot() => ParentId == null;

    public int GetLevel() => Code.Split('.').Length;

    public bool IsChildOf(OrganizationUnit ou)
    {
        ArgumentNullException.ThrowIfNull(ou);
        return Code.StartsWith(ou.Code + ".", StringComparison.Ordinal);
    }

    public bool IsDescendantOf(OrganizationUnit ou)
    {
        ArgumentNullException.ThrowIfNull(ou);
        return Code.StartsWith(ou.Code, StringComparison.Ordinal) && Code != ou.Code;
    }

}

/// <summary>
/// Many-to-many relationship between Users and Organization Units
/// </summary>
public class UserOrganizationUnit
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public int OrganizationUnitId { get; set; }
    public virtual OrganizationUnit OrganizationUnit { get; set; } = null!;

    /// <summary>
    /// Is this the user's default/primary OU?
    /// </summary>
    public bool IsDefault { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

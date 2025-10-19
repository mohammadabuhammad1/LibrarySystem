
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Application.OrganiztionUnits.Dtos;

public class OrganizationUnitDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public string? ParentCode { get; set; }
    public string? ParentDisplayName { get; set; }
    public bool IsActive { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public int? MaxLibraries { get; set; }
    public int? MaxUsers { get; set; }
    public int Level { get; set; }
    public int ChildrenCount { get; set; }
    public int LibrariesCount { get; set; }
    public int UsersCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
public class CreateOrganizationUnitDto
{
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }

    [MaxLength(50)]
    public string Type { get; set; } = "Branch";

    [EmailAddress]
    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public int? MaxLibraries { get; set; }
    public int? MaxUsers { get; set; }
}

public class UpdateOrganizationUnitDto
{
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(50)]
    public string Type { get; set; } = "Branch";

    [EmailAddress]
    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public int? MaxLibraries { get; set; }
    public int? MaxUsers { get; set; }
}

public class OrganizationUnitTreeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Level { get; set; }
    public int LibrariesCount { get; set; }
    public int UsersCount { get; set; }

    private readonly List<OrganizationUnitTreeDto> _children = new();
    public IReadOnlyCollection<OrganizationUnitTreeDto> Children => _children;

    public void AddChild(OrganizationUnitTreeDto child) => _children.Add(child);
}

public class AssignUserToOuDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int OrganizationUnitId { get; set; }

    public bool IsDefault { get; set; }
}

public class AssignLibraryToOuDto
{
    [Required]
    public int LibraryId { get; set; }

    [Required]
    public int OrganizationUnitId { get; set; }
}

public class OrganizationUnitStatsDto
{
    public int TotalOrganizationUnits { get; set; }
    public int RootOrganizationUnits { get; set; }
    public int ActiveOrganizationUnits { get; set; }
    public int TotalLibraries { get; set; }
    public int TotalUsers { get; set; }

    private readonly List<OuTypeStatDto> _byType = new();
    public IReadOnlyCollection<OuTypeStatDto> ByType => _byType;

    public OrganizationUnitStatsDto SetByType(IEnumerable<OuTypeStatDto> items)
    {
        _byType.Clear();
        _byType.AddRange(items);
        return this; // Return this for fluent API
    }
}

public class OuTypeStatDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
}

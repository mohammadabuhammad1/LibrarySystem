using Microsoft.AspNetCore.Identity;

namespace LibrarySystem.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime MembershipDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // NEW: Organization Unit relationships
    public virtual ICollection<UserOrganizationUnit> UserOrganizationUnits { get; private set; } = [];

    // Helper method to get primary OU
    public OrganizationUnit? GetPrimaryOrganizationUnit()
    {
        return UserOrganizationUnits
            .FirstOrDefault(uou => uou.IsDefault)
            ?.OrganizationUnit;
    }

    // Helper method to check if user belongs to OU
    public bool BelongsToOrganizationUnit(int ouId)
    {
        return UserOrganizationUnits.Any(uou => uou.OrganizationUnitId == ouId);
    }

    // Navigation property
    public ICollection<BorrowRecord> BorrowRecords { get;  } = [];
    public  ICollection<IdentityUserRole<string>> UserRoles { get; } = [];
}
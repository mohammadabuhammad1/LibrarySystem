using LibrarySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Data;

public static class OrganizationUnitSeeder
{
    public static async Task SeedAsync(LibraryDbContext context)
    {
        
        ArgumentNullException.ThrowIfNull(context);

        // Check if already seeded
        if (await context.OrganizationUnits.AnyAsync().ConfigureAwait(false))
            return;

        // Create a root tenant organization
        var tenant1 = new OrganizationUnit
        {
            Code = "0001",
            DisplayName = "Main Library System",
            Description = "Primary library organization",
            Type = "Tenant",
            IsActive = true,
            SubscriptionStartDate = DateTime.UtcNow,
            SubscriptionEndDate = DateTime.UtcNow.AddYears(1),
            MaxLibraries = 10,
            MaxUsers = 100,
            ContactEmail = "admin@mainlibrary.com",
            ContactPhone = "+1234567890"
        };

        await context.OrganizationUnits.AddAsync(tenant1).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        // Create branch under tenant
        var branch1 = new OrganizationUnit
        {
            Code = "0001.0001",
            DisplayName = "Downtown Branch",
            Description = "Downtown library branch",
            Type = "Branch",
            ParentId = tenant1.Id,
            IsActive = true,
            ContactEmail = "downtown@mainlibrary.com"
        };

        var branch2 = new OrganizationUnit
        {
            Code = "0001.0002",
            DisplayName = "University Branch",
            Description = "University campus library",
            Type = "Branch",
            ParentId = tenant1.Id,
            IsActive = true,
            ContactEmail = "university@mainlibrary.com"
        };

        await context.OrganizationUnits.AddRangeAsync(branch1, branch2).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        // Create sub-branch
        var subBranch = new OrganizationUnit
        {
            Code = "0001.0001.0001",
            DisplayName = "Downtown - Children's Section",
            Description = "Children's section of downtown branch",
            Type = "Department",
            ParentId = branch1.Id,
            IsActive = true
        };

        await context.OrganizationUnits.AddAsync(subBranch).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

    }
}
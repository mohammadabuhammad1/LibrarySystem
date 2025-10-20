using LibrarySystem.Domain.Entities;
using LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Services;

public class UserOrganizationUnitService(LibraryDbContext context)
{
    public async Task AssociateUserWithOrganizationUnitAsync(string userId, int organizationUnitId, bool isDefault = true)
    {
        ArgumentNullException.ThrowIfNull(userId);

        bool existingAssociation = await context.UserOrganizationUnits
            .AnyAsync(uou => uou.UserId == userId && uou.OrganizationUnitId == organizationUnitId)
            .ConfigureAwait(false);

        if (!existingAssociation)
        {
            UserOrganizationUnit userOu = new()
            {
                UserId = userId,
                OrganizationUnitId = organizationUnitId,
                IsDefault = isDefault,
                AssignedAt = DateTime.UtcNow
            };

            await context.UserOrganizationUnits.AddAsync(userOu).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task<bool> RemoveUserFromOrganizationUnitAsync(string userId, int organizationUnitId)
    {
        UserOrganizationUnit? userOu = await context.UserOrganizationUnits
            .FirstOrDefaultAsync(uou => uou.UserId == userId && uou.OrganizationUnitId == organizationUnitId)
            .ConfigureAwait(false);

        if (userOu != null)
        {
            context.UserOrganizationUnits.Remove(userOu);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        return false;
    }

    public async Task<IEnumerable<OrganizationUnit>> GetUserOrganizationUnitsAsync(string userId)
    {
        return await context.UserOrganizationUnits
            .Where(uou => uou.UserId == userId)
            .Include(uou => uou.OrganizationUnit)
            .Select(uou => uou.OrganizationUnit)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
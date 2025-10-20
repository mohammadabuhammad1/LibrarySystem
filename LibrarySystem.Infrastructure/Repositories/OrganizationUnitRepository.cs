using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Repositories;

public class OrganizationUnitRepository(LibraryDbContext context) : IOrganizationUnitRepository
{
    public async Task<OrganizationUnit?> GetByIdAsync(int id)
    {
        return await context.OrganizationUnits
            .Include(ou => ou.Parent)
            .Include(ou => ou.Children)
            .Include(ou => ou.Libraries)
            .Include(ou => ou.UserOrganizationUnits)
                .ThenInclude(uou => uou.User)
            .FirstOrDefaultAsync(ou => ou.Id == id).ConfigureAwait(false);
    }

    public async Task<OrganizationUnit?> GetByCodeAsync(string code)
    {
        return await context.OrganizationUnits
            .Include(ou => ou.Parent)
            .Include(ou => ou.Children)
            .FirstOrDefaultAsync(ou => ou.Code == code).ConfigureAwait(false);
    }

    public async Task<OrganizationUnit?> GetByNameAsync(string name)
    {
        return await context.OrganizationUnits
            .Include(ou => ou.Parent)
            .Include(ou => ou.Children)
            .FirstOrDefaultAsync(ou => ou.DisplayName == name).ConfigureAwait(false);
    }

    public async Task<IEnumerable<OrganizationUnit>> GetAllAsync()
    {
        return await context.OrganizationUnits
            .Include(ou => ou.Parent)
            .OrderBy(ou => ou.Code)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<OrganizationUnit>> GetRootOrganizationUnitsAsync()
    {
        return await context.OrganizationUnits
            .Where(ou => ou.ParentId == null)
            .OrderBy(ou => ou.Code)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<OrganizationUnit>> GetChildrenAsync(int parentId)
    {
        return await context.OrganizationUnits
            .Where(ou => ou.ParentId == parentId)
            .OrderBy(ou => ou.Code)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<OrganizationUnit>> GetDescendantsAsync(int ouId)
    {
        OrganizationUnit? ou = await GetByIdAsync(ouId).ConfigureAwait(false);
        if (ou == null)
            return Enumerable.Empty<OrganizationUnit>();

        // Get all OUs whose code starts with this OU's code
        return await context.OrganizationUnits
            .Where(o => o.Code.StartsWith(ou.Code) && o.Id != ouId)
            .OrderBy(o => o.Code)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<OrganizationUnit> AddAsync(OrganizationUnit ou)
    {
        await context.OrganizationUnits.AddAsync(ou).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
        return ou;
    }

    public async Task UpdateAsync(OrganizationUnit ou)
    {
        ArgumentNullException.ThrowIfNull(ou);

        ou.UpdatedAt = DateTime.UtcNow;
        context.OrganizationUnits.Update(ou);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteAsync(int id)
    {
        OrganizationUnit? ou = await context.OrganizationUnits.FindAsync(id).ConfigureAwait(false);
        if (ou != null)
        {
            context.OrganizationUnits.Remove(ou);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await context.OrganizationUnits.AnyAsync(ou => ou.Id == id).ConfigureAwait(false);
    }

    public async Task<bool> CodeExistsAsync(string code)
    {
        return await context.OrganizationUnits.AnyAsync(ou => ou.Code == code).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersInOrganizationUnitAsync(int ouId, bool includeDescendants = false)
    {
        IQueryable<UserOrganizationUnit> query = context.UserOrganizationUnits
            .Where(uou => uou.OrganizationUnitId == ouId);

        if (includeDescendants)
        {
            OrganizationUnit? ou = await GetByIdAsync(ouId).ConfigureAwait(false);
            if (ou != null)
            {
                List<int> descendantIds = await context.OrganizationUnits
                    .Where(o => o.Code.StartsWith(ou.Code))
                    .Select(o => o.Id)
                    .ToListAsync().ConfigureAwait(false);

                query = context.UserOrganizationUnits
                    .Where(uou => descendantIds.Contains(uou.OrganizationUnitId));
            }
        }

        return await query
            .Select(uou => uou.User)
            .Distinct()
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Library>> GetLibrariesInOrganizationUnitAsync(int ouId, bool includeDescendants = false)
    {
        if (includeDescendants)
        {
            OrganizationUnit? ou = await GetByIdAsync(ouId).ConfigureAwait(false);
            if (ou == null)
                return Enumerable.Empty<Library>();

            List<int> descendantIds = await context.OrganizationUnits
                .Where(o => o.Code.StartsWith(ou.Code))
                .Select(o => o.Id)
                .ToListAsync().ConfigureAwait(false);

            return await context.Libraries
                .Where(l => descendantIds.Contains(l.OrganizationUnitId))
                .ToListAsync().ConfigureAwait(false);
        }

        return await context.Libraries
            .Where(l => l.OrganizationUnitId == ouId)
            .ToListAsync().ConfigureAwait(false);
    }
}
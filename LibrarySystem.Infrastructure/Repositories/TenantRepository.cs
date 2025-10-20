using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibrarySystem.Infrastructure.Repositories;

public class TenantRepository(LibraryDbContext context) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(int id)
    {
        return await context.Tenants
            .Include(t => t.OrganizationUnit)
            .Include(t => t.Features)
            .FirstOrDefaultAsync(t => t.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<Tenant?> GetByCodeAsync(string code)
    {
        return await context.Tenants
            .Include(t => t.OrganizationUnit)
            .Include(t => t.Features)
            .FirstOrDefaultAsync(t => t.Code == code)
            .ConfigureAwait(false);
    }

    public async Task<Tenant?> GetByOrganizationUnitIdAsync(int ouId)
    {
        return await context.Tenants
            .Include(t => t.OrganizationUnit)
            .Include(t => t.Features)
            .FirstOrDefaultAsync(t => t.OrganizationUnitId == ouId)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await context.Tenants
            .Include(t => t.OrganizationUnit)
            .Include(t => t.Features)
            .OrderBy(t => t.DisplayName)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Tenant>> GetActiveAsync()
    {
        return await context.Tenants
            .Include(t => t.OrganizationUnit)
            .Include(t => t.Features)
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayName)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Tenant> AddAsync(Tenant tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        tenant.CreatedAt = DateTime.UtcNow;
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Tenant> entityEntry = await context.Tenants.AddAsync(tenant).ConfigureAwait(false);
        return entityEntry.Entity;
    }

    public Task<Tenant> UpdateAsync(Tenant tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        tenant.UpdatedAt = DateTime.UtcNow;
        context.Tenants.Update(tenant);
        return Task.FromResult(tenant);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Tenant? tenant = await GetByIdAsync(id).ConfigureAwait(false);
        if (tenant == null) return false;

        context.Tenants.Remove(tenant);
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await context.Tenants.AnyAsync(t => t.Id == id).ConfigureAwait(false);
    }

    public async Task<bool> CodeExistsAsync(string code)
    {
        return await context.Tenants.AnyAsync(t => t.Code == code).ConfigureAwait(false);
    }
}
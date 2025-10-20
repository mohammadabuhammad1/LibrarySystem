using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibrarySystem.Infrastructure.Repositories;

public class TenantFeatureRepository(LibraryDbContext context) : ITenantFeatureRepository
{
    public async Task<TenantFeature?> GetAsync(int tenantId, string featureName)
    {
        return await context.TenantFeatures
            .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Name == featureName)
            .ConfigureAwait(false);
    }

    public async Task<TenantFeature> SetAsync(int tenantId, string featureName, string value)
    {
        TenantFeature? existingFeature = await GetAsync(tenantId, featureName).ConfigureAwait(false);

        if (existingFeature != null)
        {
            existingFeature.Value = value;
            existingFeature.UpdatedAt = DateTime.UtcNow;
            context.TenantFeatures.Update(existingFeature);
            return existingFeature;
        }
        else
        {
            TenantFeature newFeature = new TenantFeature
            {
                TenantId = tenantId,
                Name = featureName,
                Value = value,
                CreatedAt = DateTime.UtcNow
            };
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TenantFeature> entityEntry = await context.TenantFeatures.AddAsync(newFeature).ConfigureAwait(false);
            return entityEntry.Entity;
        }
    }

    public async Task<bool> RemoveAsync(int tenantId, string featureName)
    {
        TenantFeature? feature = await GetAsync(tenantId, featureName).ConfigureAwait(false);
        if (feature == null) return false;

        context.TenantFeatures.Remove(feature);
        return true;
    }

    public async Task<IEnumerable<TenantFeature>> GetByTenantAsync(int tenantId)
    {
        return await context.TenantFeatures
            .Where(f => f.TenantId == tenantId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(int tenantId, string featureName)
    {
        return await context.TenantFeatures
            .AnyAsync(f => f.TenantId == tenantId && f.Name == featureName)
            .ConfigureAwait(false);
    }
}
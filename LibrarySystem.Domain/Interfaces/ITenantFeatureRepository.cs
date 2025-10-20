using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Domain.Interfaces;

public interface ITenantFeatureRepository
{
    Task<TenantFeature?> GetAsync(int tenantId, string featureName);
    Task<TenantFeature> SetAsync(int tenantId, string featureName, string value);
    Task<bool> RemoveAsync(int tenantId, string featureName);
    Task<IEnumerable<TenantFeature>> GetByTenantAsync(int tenantId);
    Task<bool> ExistsAsync(int tenantId, string featureName);
}
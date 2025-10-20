using LibrarySystem.Application.Dtos.OrganizationUnits;

namespace LibrarySystem.Application.Interfaces;

public interface ITenantService
{
    // Tenant Management
    Task<TenantDto> CreateTenantAsync(CreateTenantDto dto);
    Task<TenantDto> UpdateTenantAsync(int tenantId, UpdateTenantDto dto);
    Task<bool> DeleteTenantAsync(int tenantId);
    Task<TenantDto?> GetTenantAsync(int tenantId);
    Task<TenantDto?> GetTenantByCodeAsync(string code);
    Task<IEnumerable<TenantDto>> GetAllTenantsAsync();
    Task<IEnumerable<TenantDto>> GetActiveTenantsAsync();
    Task<bool> ActivateTenantAsync(int tenantId);
    Task<bool> DeactivateTenantAsync(int tenantId);

    // Tenant Features
    Task<bool> IsFeatureEnabledAsync(int tenantId, string featureName);
    Task SetFeatureValueAsync(int tenantId, string featureName, string value);
    Task<string?> GetFeatureValueAsync(int tenantId, string featureName);

    // Tenant Statistics
    Task<TenantStatsDto> GetTenantStatsAsync(int tenantId);
    Task<bool> CanCreateLibraryAsync(int tenantId);
    Task<bool> CanCreateUserAsync(int tenantId);

    // Tenant Connection Strings
    Task SetConnectionStringAsync(int tenantId, string connectionString);
    Task<string?> GetConnectionStringAsync(int tenantId);
    Task RemoveConnectionStringAsync(int tenantId);
}
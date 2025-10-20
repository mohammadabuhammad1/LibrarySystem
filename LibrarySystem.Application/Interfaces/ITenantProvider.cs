namespace LibrarySystem.Application.Interfaces;

public interface ITenantProvider
{
    int? GetCurrentTenantId();
    string? GetCurrentTenantCode();
    Task<bool> SetCurrentTenantAsync(int tenantId);
    Task<bool> SetCurrentTenantByCodeAsync(string code);
    void ClearCurrentTenant();
}
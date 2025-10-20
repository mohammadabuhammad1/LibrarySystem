using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Domain.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(int id);
    Task<Tenant?> GetByCodeAsync(string code);
    Task<Tenant?> GetByOrganizationUnitIdAsync(int ouId);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<IEnumerable<Tenant>> GetActiveAsync();
    Task<Tenant> AddAsync(Tenant tenant);
    Task<Tenant> UpdateAsync(Tenant tenant);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> CodeExistsAsync(string code);
}
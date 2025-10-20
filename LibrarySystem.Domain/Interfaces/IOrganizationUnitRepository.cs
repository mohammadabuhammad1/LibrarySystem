using LibrarySystem.Domain.Entities;


namespace LibrarySystem.Domain.Interfaces;

public interface IOrganizationUnitRepository
{
    Task<OrganizationUnit?> GetByIdAsync(int id);
    Task<OrganizationUnit?> GetByCodeAsync(string code);
    Task<OrganizationUnit?> GetByNameAsync(string name);
    Task<IEnumerable<OrganizationUnit>> GetAllAsync();
    Task<IEnumerable<OrganizationUnit>> GetRootOrganizationUnitsAsync();
    Task<IEnumerable<OrganizationUnit>> GetChildrenAsync(int parentId);
    Task<IEnumerable<OrganizationUnit>> GetDescendantsAsync(int ouId);
    Task<OrganizationUnit> AddAsync(OrganizationUnit ou);
    Task UpdateAsync(OrganizationUnit ou);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> CodeExistsAsync(string code);
    Task<IEnumerable<ApplicationUser>> GetUsersInOrganizationUnitAsync(int ouId, bool includeDescendants = false);
    Task<IEnumerable<Library>> GetLibrariesInOrganizationUnitAsync(int ouId, bool includeDescendants = false);

}

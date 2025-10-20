using LibrarySystem.Application.OrganiztionUnits.Dtos;
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Interfaces;

public interface IOrganizationUnitService
{
    Task<OrganizationUnitDto?> GetByIdAsync(int id);
    Task<OrganizationUnitDto?> GetByCodeAsync(string code);
    Task<IEnumerable<OrganizationUnitDto>> GetAllAsync();
    Task<IEnumerable<OrganizationUnitDto>> GetRootOrganizationUnitsAsync();
    Task<IEnumerable<OrganizationUnitDto>> GetChildrenAsync(int parentId);
    Task<OrganizationUnitTreeDto> GetTreeAsync(int? rootId = null);
    Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitDto dto);
    Task<OrganizationUnitDto> UpdateAsync(int id, UpdateOrganizationUnitDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> AssignUserToOuAsync(string userId, int ouId, bool isDefault = false);
    Task<bool> RemoveUserFromOuAsync(string userId, int ouId);
    Task<IEnumerable<ApplicationUser>> GetUsersInOuAsync(int ouId, bool includeDescendants = false);
    Task<IEnumerable<Library>> GetLibrariesInOuAsync(int ouId, bool includeDescendants = false);
    Task<OrganizationUnitStatsDto> GetStatsAsync();
    Task<bool> CanCreateLibraryAsync(int ouId);
    Task<bool> CanCreateUserAsync(int ouId);


    Task<OrganizationUnitDto> CreateBranchAsync(int parentTenantId, CreateOrganizationUnitDto dto);
    Task<IEnumerable<OrganizationUnitDto>> GetTenantBranchesAsync(int tenantId);
    Task<bool> IsTenantAsync(int ouId);


}
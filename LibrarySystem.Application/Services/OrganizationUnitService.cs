using LibrarySystem.Application.Interfaces;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Application.OrganiztionUnits.Dtos;

namespace LibrarySystem.Application.Services;
public class OrganizationUnitService(
    IOrganizationUnitRepository ouRepository,
    IOrganizationUnitCodeGenerator codeGenerator,
    IUnitOfWork unitOfWork) : IOrganizationUnitService
{
    public async Task<OrganizationUnitDto?> GetByIdAsync(int id)
    {
        OrganizationUnit? ou = await unitOfWork.OrganizationUnits
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        return ou == null ? null : MapToDto(ou);
    }

    public async Task<OrganizationUnitDto?> GetByCodeAsync(string code)
    {
        OrganizationUnit? ou = await unitOfWork.OrganizationUnits
            .GetByCodeAsync(code)
            .ConfigureAwait(false);

        return ou == null ? null : MapToDto(ou);
    }

    public async Task<IEnumerable<OrganizationUnitDto>> GetAllAsync()
    {
        IEnumerable<OrganizationUnit> ous = await unitOfWork.OrganizationUnits
            .GetAllAsync()
            .ConfigureAwait(false);

        return ous.Select(MapToDto);
    }

    public async Task<IEnumerable<OrganizationUnitDto>> GetRootOrganizationUnitsAsync()
    {
        IEnumerable<OrganizationUnit> ous = await unitOfWork.OrganizationUnits
            .GetRootOrganizationUnitsAsync()
            .ConfigureAwait(false);

        return ous.Select(MapToDto);
    }

    public async Task<IEnumerable<OrganizationUnitDto>> GetChildrenAsync(int parentId)
    {
        IEnumerable<OrganizationUnit> children = await unitOfWork.OrganizationUnits
            .GetChildrenAsync(parentId)
            .ConfigureAwait(false);

        return children.Select(MapToDto);
    }

    public async Task<OrganizationUnitTreeDto> GetTreeAsync(int? rootId = null)
    {
        if (rootId.HasValue)
        {
            OrganizationUnit? ou = await unitOfWork.OrganizationUnits
                .GetByIdAsync(rootId.Value)
                .ConfigureAwait(false);

            if (ou == null)
                throw new InvalidOperationException($"OU with ID {rootId} not found");

            return await BuildTreeAsync(ou).ConfigureAwait(false);
        }

        // Build full tree from roots
        IEnumerable<OrganizationUnit> roots = await unitOfWork.OrganizationUnits
            .GetRootOrganizationUnitsAsync()
            .ConfigureAwait(false);

        var treeDto = new OrganizationUnitTreeDto
        {
            Id = 0,
            Code = "ROOT",
            DisplayName = "All Organizations",
            IsActive = true,
            Level = 0
        };

        foreach (OrganizationUnit root in roots)
        {
            treeDto.AddChild(await BuildTreeAsync(root).ConfigureAwait(false));
        }

        return treeDto;
    }

    public async Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        // Validate parent if specified
        if (dto.ParentId.HasValue)
        {
            OrganizationUnit? parent = await unitOfWork.OrganizationUnits
                .GetByIdAsync(dto.ParentId.Value)
                .ConfigureAwait(false);

            if (parent == null)
                throw new InvalidOperationException($"Parent OU with ID {dto.ParentId} not found");

            if (!parent.IsActive)
                throw new InvalidOperationException("Cannot create child under inactive OU");
        }

        // Generate next code
        var code = await codeGenerator.GenerateNextCodeAsync(dto.ParentId).ConfigureAwait(false);

        var ou = new OrganizationUnit
        {
            Code = code,
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            ParentId = dto.ParentId,
            Type = dto.Type,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            SubscriptionStartDate = dto.SubscriptionStartDate,
            SubscriptionEndDate = dto.SubscriptionEndDate,
            MaxLibraries = dto.MaxLibraries,
            MaxUsers = dto.MaxUsers,
            IsActive = true
        };

        OrganizationUnit created = await unitOfWork.OrganizationUnits
            .AddAsync(ou)
            .ConfigureAwait(false);

        // ✅ SAVE to database
        bool success = await unitOfWork.CommitAsync().ConfigureAwait(false);
        if (!success)
            throw new InvalidOperationException("Failed to create organization unit");

        return MapToDto(created);
    }

    public async Task<OrganizationUnitDto> UpdateAsync(int id, UpdateOrganizationUnitDto dto)
    {
        OrganizationUnit? ou = await unitOfWork.OrganizationUnits
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        ArgumentNullException.ThrowIfNull(dto);

        if (ou == null)
            throw new InvalidOperationException($"OU with ID {id} not found");

        ou.DisplayName = dto.DisplayName;
        ou.Description = dto.Description;
        ou.IsActive = dto.IsActive;
        ou.Type = dto.Type;
        ou.ContactEmail = dto.ContactEmail;
        ou.ContactPhone = dto.ContactPhone;
        ou.SubscriptionStartDate = dto.SubscriptionStartDate;
        ou.SubscriptionEndDate = dto.SubscriptionEndDate;
        ou.MaxLibraries = dto.MaxLibraries;
        ou.MaxUsers = dto.MaxUsers;

        await unitOfWork.OrganizationUnits
            .UpdateAsync(ou)
            .ConfigureAwait(false);

        // ✅ SAVE changes to database
        bool success = await unitOfWork.CommitAsync().ConfigureAwait(false);
        if (!success)
            throw new InvalidOperationException("Failed to update organization unit");

        return MapToDto(ou);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        OrganizationUnit? ou = await unitOfWork.OrganizationUnits
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        if (ou == null)
            return false;

        // Check if has children
        IEnumerable<OrganizationUnit> children = await unitOfWork.OrganizationUnits
            .GetChildrenAsync(id)
            .ConfigureAwait(false);

        if (children.Any())
            throw new InvalidOperationException("Cannot delete OU with children. Delete children first.");

        // Check if has libraries
        IEnumerable<Library> libraries = await unitOfWork.OrganizationUnits
            .GetLibrariesInOrganizationUnitAsync(id)
            .ConfigureAwait(false);

        if (libraries.Any())
            throw new InvalidOperationException("Cannot delete OU with libraries. Reassign or delete libraries first.");

        // Check if has users
        IEnumerable<ApplicationUser> users = await unitOfWork.OrganizationUnits
            .GetUsersInOrganizationUnitAsync(id)
            .ConfigureAwait(false);

        if (users.Any())
            throw new InvalidOperationException("Cannot delete OU with assigned users. Reassign or remove users first.");

        await unitOfWork.OrganizationUnits
            .DeleteAsync(id)
            .ConfigureAwait(false);

        // ✅ SAVE deletion to database
        bool success = await unitOfWork.CommitAsync().ConfigureAwait(false);
        if (!success)
            throw new InvalidOperationException("Failed to delete organization unit");

        return true;
    }

    public async Task<bool> AssignUserToOuAsync(string userId, int ouId, bool isDefault = false)
    {
        OrganizationUnit? ou = await unitOfWork.OrganizationUnits
            .GetByIdAsync(ouId)
            .ConfigureAwait(false);

        if (ou == null)
            throw new InvalidOperationException($"OU with ID {ouId} not found");

        if (!ou.IsActive)
            throw new InvalidOperationException("Cannot assign user to inactive OU");

        // Check if can add more users
        if (!await CanCreateUserAsync(ouId).ConfigureAwait(false))
            throw new InvalidOperationException("Maximum user limit reached for this OU");

        // Implementation would be in repository
        // This is simplified - you'd need UserOrganizationUnit repository

        // ✅ If you add user assignment logic here, remember to call CommitAsync()
        bool success = await unitOfWork.CommitAsync().ConfigureAwait(false);
        if (!success)
            throw new InvalidOperationException("Failed to assign user to organization unit");

        return true;
    }

    public async Task<bool> RemoveUserFromOuAsync(string userId, int ouId)
    {
        // Implementation in repository
        // If you implement this, remember to call CommitAsync()

        bool success = await unitOfWork.CommitAsync().ConfigureAwait(false);
        if (!success)
            throw new InvalidOperationException("Failed to remove user from organization unit");

        return true;
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersInOuAsync(int ouId, bool includeDescendants = false)
    {
        return await unitOfWork.OrganizationUnits
            .GetUsersInOrganizationUnitAsync(ouId, includeDescendants)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Library>> GetLibrariesInOuAsync(int ouId, bool includeDescendants = false)
    {
        return await unitOfWork.OrganizationUnits
            .GetLibrariesInOrganizationUnitAsync(ouId, includeDescendants)
            .ConfigureAwait(false);
    }

    public async Task<OrganizationUnitStatsDto> GetStatsAsync()
    {
        IEnumerable<OrganizationUnit> allOus = await unitOfWork.OrganizationUnits
            .GetAllAsync()
            .ConfigureAwait(false);

        var allOusList = allOus.ToList();

        var byType = allOusList
            .GroupBy(ou => ou.Type)
            .Select(g => new OuTypeStatDto
            {
                Type = g.Key,
                Count = g.Count()
            })
            .ToList();

        return new OrganizationUnitStatsDto
        {
            TotalOrganizationUnits = allOusList.Count,
            RootOrganizationUnits = allOusList.Count(ou => ou.ParentId == null),
            ActiveOrganizationUnits = allOusList.Count(ou => ou.IsActive),
            TotalLibraries = allOusList.Sum(ou => ou.Libraries?.Count ?? 0),
            TotalUsers = allOusList.Sum(ou => ou.UserOrganizationUnits?.Count ?? 0)
        }.SetByType(byType);
    }

    public async Task<bool> CanCreateLibraryAsync(int ouId)
    {
        OrganizationUnit? ou = await unitOfWork.OrganizationUnits
            .GetByIdAsync(ouId)
            .ConfigureAwait(false);

        if (ou == null || !ou.IsActive)
            return false;

        if (!ou.MaxLibraries.HasValue)
            return true; // No limit

        IEnumerable<Library> libraries = await unitOfWork.OrganizationUnits
            .GetLibrariesInOrganizationUnitAsync(ouId)
            .ConfigureAwait(false);

        return libraries.Count() < ou.MaxLibraries.Value;
    }

    public async Task<bool> CanCreateUserAsync(int ouId)
    {
        OrganizationUnit? ou = await unitOfWork.OrganizationUnits
            .GetByIdAsync(ouId)
            .ConfigureAwait(false);

        if (ou == null || !ou.IsActive)
            return false;

        if (!ou.MaxUsers.HasValue)
            return true; // No limit

        IEnumerable<ApplicationUser> users = await unitOfWork.OrganizationUnits
            .GetUsersInOrganizationUnitAsync(ouId)
            .ConfigureAwait(false);

        return users.Count() < ou.MaxUsers.Value;
    }

     public async Task<OrganizationUnitDto> CreateBranchAsync(int parentTenantId, CreateOrganizationUnitDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        // Verify the parent is a tenant (root level)
        OrganizationUnit? parentOu = await ouRepository.GetByIdAsync(parentTenantId).ConfigureAwait(false);
        if (parentOu == null)
            throw new InvalidOperationException($"Parent organization unit with ID {parentTenantId} not found");

        if (parentOu.ParentId != null)
            throw new InvalidOperationException("Parent organization unit must be a tenant (root level)");

        // Generate the next code for the branch
        string branchCode = await codeGenerator.GenerateNextCodeAsync(parentTenantId).ConfigureAwait(false);

        // Create the branch organization unit
        var branch = new OrganizationUnit
        {
            Code = branchCode,
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            ParentId = parentTenantId,
            Type = "Branch", // Set type as Branch
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            SubscriptionStartDate = dto.SubscriptionStartDate,
            SubscriptionEndDate = dto.SubscriptionEndDate,
            MaxLibraries = dto.MaxLibraries,
            MaxUsers = dto.MaxUsers,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        OrganizationUnit createdBranch = await ouRepository.AddAsync(branch).ConfigureAwait(false);

        // Map to DTO and return
        return MapToDto(createdBranch);
    }

    public async Task<IEnumerable<OrganizationUnitDto>> GetTenantBranchesAsync(int tenantId)
    {
        // Get all direct children of the tenant
        IEnumerable<OrganizationUnit> branches = await ouRepository.GetChildrenAsync(tenantId).ConfigureAwait(false);
        
        return branches.Select(MapToDto);
    }

    public async Task<bool> IsTenantAsync(int ouId)
    {
        OrganizationUnit? ou = await ouRepository.GetByIdAsync(ouId).ConfigureAwait(false);
        return ou?.ParentId == null && ou?.Type == "Tenant";
    }

    private async Task<OrganizationUnitTreeDto> BuildTreeAsync(OrganizationUnit ou)
    {
        IEnumerable<OrganizationUnit> children = await unitOfWork.OrganizationUnits
            .GetChildrenAsync(ou.Id)
            .ConfigureAwait(false);

        var treeDto = new OrganizationUnitTreeDto
        {
            Id = ou.Id,
            Code = ou.Code,
            DisplayName = ou.DisplayName,
            Description = ou.Description,
            IsActive = ou.IsActive,
            Level = ou.GetLevel(),
            LibrariesCount = ou.Libraries?.Count ?? 0,
            UsersCount = ou.UserOrganizationUnits?.Count ?? 0
        };

        foreach (OrganizationUnit child in children)
        {
            treeDto.AddChild(await BuildTreeAsync(child).ConfigureAwait(false));
        }

        return treeDto;
    }

    private OrganizationUnitDto MapToDto(OrganizationUnit ou)
    {
        return new OrganizationUnitDto
        {
            Id = ou.Id,
            Code = ou.Code,
            DisplayName = ou.DisplayName,
            Description = ou.Description,
            ParentId = ou.ParentId,
            ParentCode = ou.Parent?.Code,
            ParentDisplayName = ou.Parent?.DisplayName,
            IsActive = ou.IsActive,
            Type = ou.Type,
            ContactEmail = ou.ContactEmail,
            ContactPhone = ou.ContactPhone,
            SubscriptionStartDate = ou.SubscriptionStartDate,
            SubscriptionEndDate = ou.SubscriptionEndDate,
            MaxLibraries = ou.MaxLibraries,
            MaxUsers = ou.MaxUsers,
            Level = ou.GetLevel(),
            ChildrenCount = ou.Children?.Count ?? 0,
            LibrariesCount = ou.Libraries?.Count ?? 0,
            UsersCount = ou.UserOrganizationUnits?.Count ?? 0,
            CreatedAt = ou.CreatedAt,
            UpdatedAt = ou.UpdatedAt
        };
    }
}
using LibrarySystem.Application.Dtos.OrganizationUnits;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;

namespace LibrarySystem.Application.Services;

public class TenantService(IUnitOfWork unitOfWork, IOrganizationUnitCodeGenerator codeGenerator) : ITenantService
{
    public async Task<TenantDto> CreateTenantAsync(CreateTenantDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        OrganizationUnit? existingOu = await unitOfWork.OrganizationUnits.GetByNameAsync(dto.DisplayName).ConfigureAwait(false);
        if (existingOu != null)
            throw new InvalidOperationException($"Tenant with name '{dto.DisplayName}' already exists");

        // Create root organization unit as tenant
        string code = await codeGenerator.GenerateNextCodeAsync().ConfigureAwait(false);

        OrganizationUnit tenantOu = new OrganizationUnit
        {
            Code = code,
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            Type = "Tenant",
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            SubscriptionStartDate = dto.SubscriptionStartDate,
            SubscriptionEndDate = dto.SubscriptionEndDate,
            MaxLibraries = dto.MaxLibraries,
            MaxUsers = dto.MaxUsers,
            IsActive = true,
            ParentId = null
        };

        OrganizationUnit created = await unitOfWork.OrganizationUnits.AddAsync(tenantOu).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        return MapToDto(created);
    }

    public async Task<TenantDto> UpdateTenantAsync(int tenantId, UpdateTenantDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        OrganizationUnit? tenantOu = await unitOfWork.OrganizationUnits.GetByIdAsync(tenantId).ConfigureAwait(false);
        if (tenantOu == null || tenantOu.Type != "Tenant")
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found");

        tenantOu.DisplayName = dto.DisplayName;
        tenantOu.Description = dto.Description;
        tenantOu.ContactEmail = dto.ContactEmail;
        tenantOu.ContactPhone = dto.ContactPhone;
        tenantOu.SubscriptionStartDate = dto.SubscriptionStartDate;
        tenantOu.SubscriptionEndDate = dto.SubscriptionEndDate;
        tenantOu.MaxLibraries = dto.MaxLibraries;
        tenantOu.MaxUsers = dto.MaxUsers;
        tenantOu.IsActive = dto.IsActive;
        tenantOu.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.OrganizationUnits.UpdateAsync(tenantOu).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        return MapToDto(tenantOu);
    }

    public async Task<bool> DeleteTenantAsync(int tenantId)
    {
        OrganizationUnit? tenantOu = await unitOfWork.OrganizationUnits.GetByIdAsync(tenantId).ConfigureAwait(false);
        if (tenantOu == null || tenantOu.Type != "Tenant")
            return false;

        // Check if tenant has libraries
        IEnumerable<Library> libraries = await unitOfWork.OrganizationUnits.GetLibrariesInOrganizationUnitAsync(tenantId).ConfigureAwait(false);
        if (libraries.Any())
            throw new InvalidOperationException("Cannot delete tenant with libraries");

        // Check if tenant has users
        IEnumerable<ApplicationUser> users = await unitOfWork.OrganizationUnits.GetUsersInOrganizationUnitAsync(tenantId).ConfigureAwait(false);
        if (users.Any())
            throw new InvalidOperationException("Cannot delete tenant with assigned users");

        // Check if tenant has children (branches/departments)
        IEnumerable<OrganizationUnit> children = await unitOfWork.OrganizationUnits.GetChildrenAsync(tenantId).ConfigureAwait(false);
        if (children.Any())
            throw new InvalidOperationException("Cannot delete tenant with child organization units");

        await unitOfWork.OrganizationUnits.DeleteAsync(tenantId).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<TenantDto?> GetTenantAsync(int tenantId)
    {
        OrganizationUnit? tenantOu = await unitOfWork.OrganizationUnits.GetByIdAsync(tenantId).ConfigureAwait(false);
        return tenantOu?.Type == "Tenant" ? MapToDto(tenantOu) : null;
    }

    public async Task<TenantDto?> GetTenantByCodeAsync(string code)
    {
        OrganizationUnit? tenantOu = await unitOfWork.OrganizationUnits.GetByCodeAsync(code).ConfigureAwait(false);
        return tenantOu?.Type == "Tenant" ? MapToDto(tenantOu) : null;
    }

    public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync()
    {
        IEnumerable<OrganizationUnit> rootOUs = await unitOfWork.OrganizationUnits.GetRootOrganizationUnitsAsync().ConfigureAwait(false);
        IEnumerable<OrganizationUnit> tenants = rootOUs.Where(ou => ou.Type == "Tenant");
        return tenants.Select(MapToDto);
    }

    public async Task<IEnumerable<TenantDto>> GetActiveTenantsAsync()
    {
        IEnumerable<OrganizationUnit> rootOUs = await unitOfWork.OrganizationUnits.GetRootOrganizationUnitsAsync().ConfigureAwait(false);
        IEnumerable<OrganizationUnit> activeTenants = rootOUs.Where(ou => ou.Type == "Tenant" && ou.IsActive);
        return activeTenants.Select(MapToDto);
    }

    public async Task<bool> ActivateTenantAsync(int tenantId)
    {
        OrganizationUnit? tenantOu = await unitOfWork.OrganizationUnits.GetByIdAsync(tenantId).ConfigureAwait(false);
        if (tenantOu?.Type != "Tenant") return false;

        tenantOu.IsActive = true;
        tenantOu.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.OrganizationUnits.UpdateAsync(tenantOu).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<bool> DeactivateTenantAsync(int tenantId)
    {
        OrganizationUnit? tenantOu = await unitOfWork.OrganizationUnits.GetByIdAsync(tenantId).ConfigureAwait(false);
        if (tenantOu?.Type != "Tenant") return false;

        tenantOu.IsActive = false;
        tenantOu.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.OrganizationUnits.UpdateAsync(tenantOu).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        return true;
    }

    public Task<bool> IsFeatureEnabledAsync(int tenantId, string featureName)
    {
        Dictionary<string, bool> defaultFeatures = new()
        {
            ["AdvancedReporting"] = true,
            ["MultipleLibraries"] = true,
            ["UserManagement"] = true,
            ["CustomBranding"] = false
        };

        return Task.FromResult(defaultFeatures.GetValueOrDefault(featureName, false));
    }

    public Task SetFeatureValueAsync(int tenantId, string featureName, string value)
    {
        // Store in a tenant_features table or use configuration
        return Task.CompletedTask;
    }

    public Task<string?> GetFeatureValueAsync(int tenantId, string featureName)
    {
        return Task.FromResult<string?>(null);
    }

    public async Task<TenantStatsDto> GetTenantStatsAsync(int tenantId)
    {
        OrganizationUnit? tenantOu = await unitOfWork.OrganizationUnits.GetByIdAsync(tenantId).ConfigureAwait(false);
        if (tenantOu?.Type != "Tenant")
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found");

        // Get all libraries under this tenant (including branches)
        IEnumerable<Library> allLibraries = await unitOfWork.OrganizationUnits.GetLibrariesInOrganizationUnitAsync(tenantId, includeDescendants: true).ConfigureAwait(false);

        // Get all users under this tenant
        IEnumerable<ApplicationUser> allUsers = await unitOfWork.OrganizationUnits.GetUsersInOrganizationUnitAsync(tenantId, includeDescendants: true).ConfigureAwait(false);

        // Calculate book statistics
        int totalBooks = 0;
        int borrowedBooks = 0;

        foreach (Library library in allLibraries)
        {
            IEnumerable<Book> libraryBooks = await unitOfWork.Books.GetBooksByLibraryAsync(library.Id).ConfigureAwait(false);
            totalBooks += libraryBooks.Sum(b => b.TotalCopies);
            borrowedBooks += libraryBooks.Sum(b => b.BorrowedCopiesCount);
        }

        return new TenantStatsDto
        {
            TotalLibraries = allLibraries.Count(),
            TotalUsers = allUsers.Count(),
            TotalBooks = totalBooks,
            TotalBorrowedBooks = borrowedBooks,
            ActiveBorrows = borrowedBooks,
            TotalFines = 0
        };
    }

    public async Task<bool> CanCreateLibraryAsync(int tenantId)
    {
        OrganizationUnit? tenantOu = await unitOfWork.OrganizationUnits.GetByIdAsync(tenantId).ConfigureAwait(false);
        if (tenantOu?.Type != "Tenant") return false;

        if (!tenantOu.MaxLibraries.HasValue) return true;

        IEnumerable<Library> currentLibraries = await unitOfWork.OrganizationUnits.GetLibrariesInOrganizationUnitAsync(tenantId, includeDescendants: true).ConfigureAwait(false);
        return currentLibraries.Count() < tenantOu.MaxLibraries.Value;
    }

    public async Task<bool> CanCreateUserAsync(int tenantId)
    {
        OrganizationUnit? tenantOu = await unitOfWork.OrganizationUnits.GetByIdAsync(tenantId).ConfigureAwait(false);
        if (tenantOu?.Type != "Tenant") return false;

        if (!tenantOu.MaxUsers.HasValue) return true;

        IEnumerable<ApplicationUser> currentUsers = await unitOfWork.OrganizationUnits.GetUsersInOrganizationUnitAsync(tenantId, includeDescendants: true).ConfigureAwait(false);
        return currentUsers.Count() < tenantOu.MaxUsers.Value;
    }

    public Task SetConnectionStringAsync(int tenantId, string connectionString)
        => Task.CompletedTask;

    public Task<string?> GetConnectionStringAsync(int tenantId)
        => Task.FromResult<string?>(null);

    public Task RemoveConnectionStringAsync(int tenantId)
        => Task.CompletedTask;

    private static TenantDto MapToDto(OrganizationUnit ou)
    {
        return new TenantDto
        {
            Id = ou.Id,
            Code = ou.Code,
            DisplayName = ou.DisplayName,
            Description = ou.Description,
            IsActive = ou.IsActive,
            ContactEmail = ou.ContactEmail,
            ContactPhone = ou.ContactPhone,
            SubscriptionStartDate = ou.SubscriptionStartDate,
            SubscriptionEndDate = ou.SubscriptionEndDate,
            MaxLibraries = ou.MaxLibraries,
            MaxUsers = ou.MaxUsers,
            LibrariesCount = ou.Libraries?.Count ?? 0,
            UsersCount = ou.UserOrganizationUnits?.Count ?? 0,
            HasSeparateDatabase = false,
            CreatedAt = ou.CreatedAt,
            UpdatedAt = ou.UpdatedAt
        };
    }
}
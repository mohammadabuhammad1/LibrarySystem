using LibrarySystem.API.Errors;
using LibrarySystem.Application.Dtos.OrganizationUnits;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Application.OrganiztionUnits.Dtos;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LibrarySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("PerTenantPolicy")]
public class TenantsController(
    ITenantService tenantService,
    ITenantProvider tenantProvider,
    UserManager<ApplicationUser> userManager) : BaseApiController(userManager)
{
    /// <summary>
    /// Get all tenants
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.Admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetAllTenants()
    {
        IEnumerable<TenantDto> tenants = await tenantService.GetAllTenantsAsync().ConfigureAwait(false);
        return Ok(tenants);
    }

    /// <summary>
    /// Get active tenants only
    /// </summary>
    [HttpGet("active")]
    [Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.Admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetActiveTenants()
    {
        IEnumerable<TenantDto> tenants = await tenantService.GetActiveTenantsAsync().ConfigureAwait(false);
        return Ok(tenants);
    }

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.Admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetTenantById(int id)
    {
        TenantDto? tenant = await tenantService.GetTenantAsync(id).ConfigureAwait(false);
        if (tenant == null)
            return NotFound(new ApiResponse(404, $"Tenant with ID {id} not found"));

        return Ok(tenant);
    }

    /// <summary>
    /// Get tenant by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    [Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.Admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetTenantByCode(string code)
    {
        TenantDto? tenant = await tenantService.GetTenantByCodeAsync(code).ConfigureAwait(false);
        if (tenant == null)
            return NotFound(new ApiResponse(404, $"Tenant with code {code} not found"));

        return Ok(tenant);
    }

    /// <summary>
    /// Create a new tenant
    /// </summary>
    [HttpPost]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [EnableRateLimiting("AuthPolicy")] 
    public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] CreateTenantDto createTenantDto)
    {
        ArgumentNullException.ThrowIfNull(createTenantDto);

        try
        {
            ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
            Console.WriteLine($"Tenant '{createTenantDto.DisplayName}' created by: {currentUser?.Name}");

            TenantDto tenant = await tenantService.CreateTenantAsync(createTenantDto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetTenantById), new { id = tenant.Id }, tenant);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(400, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Update a tenant
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> UpdateTenant(int id, [FromBody] UpdateTenantDto updateTenantDto)
    {
        try
        {
            ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
            Console.WriteLine($"Tenant {id} updated by: {currentUser?.Name}");

            TenantDto tenant = await tenantService.UpdateTenantAsync(id, updateTenantDto).ConfigureAwait(false);
            return Ok(tenant);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Delete a tenant
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTenant(int id)
    {
        try
        {
            ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
            Console.WriteLine($"Tenant {id} deleted by: {currentUser?.Name}");

            bool result = await tenantService.DeleteTenantAsync(id).ConfigureAwait(false);
            if (!result)
                return NotFound(new ApiResponse(404, $"Tenant with ID {id} not found"));

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Activate a tenant
    /// </summary>
    [HttpPost("{id}/activate")]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [EnableRateLimiting("ApiPolicy")] 
    public async Task<ActionResult> ActivateTenant(int id)
    {
        bool result = await tenantService.ActivateTenantAsync(id).ConfigureAwait(false);
        if (!result)
            return NotFound(new ApiResponse(404, $"Tenant with ID {id} not found"));

        return Ok(new { message = "Tenant activated successfully" });
    }

    /// <summary>
    /// Deactivate a tenant
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateTenant(int id)
    {
        bool result = await tenantService.DeactivateTenantAsync(id).ConfigureAwait(false);
        if (!result)
            return NotFound(new ApiResponse(404, $"Tenant with ID {id} not found"));

        return Ok(new { message = "Tenant deactivated successfully" });
    }

    /// <summary>
    /// Get tenant statistics
    /// </summary>
    [HttpGet("{id}/stats")]
    [Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.Admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantStatsDto>> GetTenantStats(int id)
    {
        try
        {
            TenantStatsDto stats = await tenantService.GetTenantStatsAsync(id).ConfigureAwait(false);
            return Ok(stats);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Check if tenant can create more libraries
    /// </summary>
    [HttpGet("{id}/can-create-library")]
    [Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.Admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CanCreateLibrary(int id)
    {
        bool canCreate = await tenantService.CanCreateLibraryAsync(id).ConfigureAwait(false);
        return Ok(canCreate);
    }

    /// <summary>
    /// Check if tenant can add more users
    /// </summary>
    [HttpGet("{id}/can-create-user")]
    [Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.Admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CanCreateUser(int id)
    {
        bool canCreate = await tenantService.CanCreateUserAsync(id).ConfigureAwait(false);
        return Ok(canCreate);
    }

    /// <summary>
    /// Check if a feature is enabled for a tenant
    /// </summary>
    [HttpGet("{id}/features/{featureName}")]
    [Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.Admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> IsFeatureEnabled(int id, string featureName)
    {
        bool isEnabled = await tenantService.IsFeatureEnabledAsync(id, featureName).ConfigureAwait(false);
        return Ok(isEnabled);
    }

    /// <summary>
    /// Set a feature value for a tenant
    /// </summary>
    [HttpPost("{id}/features/{featureName}")]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SetFeatureValue(int id, string featureName, [FromBody] string value)
    {
        await tenantService.SetFeatureValueAsync(id, featureName, value).ConfigureAwait(false);
        return Ok(new { message = $"Feature '{featureName}' updated successfully" });
    }

    /// <summary>
    /// Get current tenant context (for tenant admins)
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetCurrentTenant()
    {
        try
        {
            // Get current tenant ID from tenant provider
            int? currentTenantId = tenantProvider.GetCurrentTenantId();

            if (!currentTenantId.HasValue)
            {
                // For now, return the first tenant as a fallback (for testing)
                IEnumerable<TenantDto> allTenants = await tenantService.GetAllTenantsAsync().ConfigureAwait(false);
                TenantDto? firstTenant = allTenants.FirstOrDefault();

                if (firstTenant == null)
                    return NotFound(new ApiResponse(404, "No tenants found in the system"));

                return Ok(firstTenant);
            }

            // Get tenant details
            TenantDto? tenant = await tenantService.GetTenantAsync(currentTenantId.Value).ConfigureAwait(false);
            if (tenant == null)
                return NotFound(new ApiResponse(404, $"Tenant with ID {currentTenantId} not found"));

            return Ok(tenant);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error getting current tenant: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse(500, "An error occurred while retrieving tenant information"));
        }
    }

    [HttpPost("tenants/{tenantId}/branches")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<OrganizationUnitDto>> CreateBranch(int tenantId, [FromBody] CreateOrganizationUnitDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        try
        {
            ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
            Console.WriteLine($"Branch '{dto.DisplayName}' created for tenant {tenantId} by: {currentUser?.Name}");

            OrganizationUnitDto branch = await ouService.CreateBranchAsync(tenantId, dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = branch.Id }, branch);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get all branches for a specific tenant
    /// </summary>
    [HttpGet("tenants/{tenantId}/branches")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrganizationUnitDto>>> GetTenantBranches(int tenantId)
    {
        try
        {
            // Verify the tenant exists and is actually a tenant
            bool isTenant = await ouService.IsTenantAsync(tenantId).ConfigureAwait(false);
            if (!isTenant)
                return NotFound($"Organization unit with ID {tenantId} is not a tenant");

            IEnumerable<OrganizationUnitDto> branches = await ouService.GetTenantBranchesAsync(tenantId).ConfigureAwait(false);
            return Ok(branches);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Check if an organization unit is a tenant
    /// </summary>
    [HttpGet("{id}/is-tenant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> IsTenant(int id)
    {
        bool isTenant = await ouService.IsTenantAsync(id).ConfigureAwait(false);
        return Ok(isTenant);
    }
}
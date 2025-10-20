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
public class OrganizationUnitsController(
    IOrganizationUnitService ouService,
    UserManager<ApplicationUser> userManager) : BaseApiController(userManager)
{
    /// <summary>
    /// Get all organization units
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganizationUnitDto>>> GetAll()
    {
        IEnumerable<OrganizationUnitDto> ous = await ouService.GetAllAsync().ConfigureAwait(false);
        return Ok(ous);
    }

    /// <summary>
    /// Get organization unit by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationUnitDto>> GetById(int id)
    {
        OrganizationUnitDto? ou = await ouService.GetByIdAsync(id).ConfigureAwait(false);
        if (ou == null)
            return NotFound($"Organization Unit with ID {id} not found");

        return Ok(ou);
    }

    /// <summary>
    /// Get organization unit by code (e.g., 0001.0001)
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationUnitDto>> GetByCode(string code)
    {
        OrganizationUnitDto? ou = await ouService.GetByCodeAsync(code).ConfigureAwait(false);
        if (ou == null)
            return NotFound($"Organization Unit with code {code} not found");

        return Ok(ou);
    }

    /// <summary>
    /// Get root organization units (tenants)
    /// </summary>
    [HttpGet("roots")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganizationUnitDto>>> GetRoots()
    {
        IEnumerable<OrganizationUnitDto> roots = await ouService.GetRootOrganizationUnitsAsync().ConfigureAwait(false);
        return Ok(roots);
    }

    /// <summary>
    /// Get children of an organization unit
    /// </summary>
    [HttpGet("{parentId}/children")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganizationUnitDto>>> GetChildren(int parentId)
    {
        IEnumerable<OrganizationUnitDto> children = await ouService.GetChildrenAsync(parentId).ConfigureAwait(false);
        return Ok(children);
    }

    /// <summary>
    /// Get organization unit tree structure
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganizationUnitTreeDto>> GetTree([FromQuery] int? rootId = null)
    {
        OrganizationUnitTreeDto tree = await ouService.GetTreeAsync(rootId).ConfigureAwait(false);
        return Ok(tree);
    }

    /// <summary>
    /// Create a new organization unit (tenant or branch)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<OrganizationUnitDto>> Create([FromBody] CreateOrganizationUnitDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        try
        {
            ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
            Console.WriteLine($"Organization Unit '{dto.DisplayName}' created by: {currentUser?.Name}");

            OrganizationUnitDto ou = await ouService.CreateAsync(dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = ou.Id }, ou);
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
    /// Update an organization unit
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationUnitDto>> Update(int id, [FromBody] UpdateOrganizationUnitDto dto)
    {
        try
        {
            ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
            Console.WriteLine($"Organization Unit {id} updated by: {currentUser?.Name}");

            OrganizationUnitDto ou = await ouService.UpdateAsync(id, dto).ConfigureAwait(false);
            return Ok(ou);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete an organization unit
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
            Console.WriteLine($"Organization Unit {id} deleted by: {currentUser?.Name}");

            bool result = await ouService.DeleteAsync(id).ConfigureAwait(false);
            if (!result)
                return NotFound($"Organization Unit with ID {id} not found");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get users in an organization unit
    /// </summary>
    [HttpGet("{ouId}/users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers(
        int ouId,
        [FromQuery] bool includeDescendants = false)
    {
        IEnumerable<ApplicationUser> users = await ouService.GetUsersInOuAsync(ouId, includeDescendants).ConfigureAwait(false);

        var userDtos = users.Select(u => new
        {
            u.Id,
            u.Name,
            u.Email,
            u.Phone,
            u.IsActive
        });
        return Ok(userDtos);
    }

    /// <summary>
    /// Get libraries in an organization unit
    /// </summary>
    [HttpGet("{ouId}/libraries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetLibraries(
        int ouId,
        [FromQuery] bool includeDescendants = false)
    {
        IEnumerable<Library> libraries = await ouService.GetLibrariesInOuAsync(ouId, includeDescendants).ConfigureAwait(false);
        var libraryDtos = libraries.Select(l => new
        {
            l.Id,
            l.Name,
            l.Location,
            l.Description
        });
        return Ok(libraryDtos);
    }

    /// <summary>
    /// Assign a user to an organization unit
    /// </summary>
    [HttpPost("assign-user")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult> AssignUser([FromBody] AssignUserToOuDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        try
        {
            bool result = await ouService.AssignUserToOuAsync(dto.UserId, dto.OrganizationUnitId, dto.IsDefault).ConfigureAwait(false);
            if (result)
                return Ok(new { message = "User assigned to organization unit successfully" });

            return BadRequest("Failed to assign user");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Remove a user from an organization unit
    /// </summary>
    [HttpPost("remove-user")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveUser([FromBody] AssignUserToOuDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        try
        {
            bool result = await ouService.RemoveUserFromOuAsync(dto.UserId, dto.OrganizationUnitId).ConfigureAwait(false);
            if (result)
                return Ok(new { message = "User removed from organization unit successfully" });

            return BadRequest("Failed to remove user");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get organization unit statistics
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganizationUnitStatsDto>> GetStats()
    {
        OrganizationUnitStatsDto stats = await ouService.GetStatsAsync().ConfigureAwait(false);
        return Ok(stats);
    }

    /// <summary>
    /// Check if organization unit can create more libraries
    /// </summary>
    [HttpGet("{ouId}/can-create-library")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CanCreateLibrary(int ouId)
    {
        bool canCreate = await ouService.CanCreateLibraryAsync(ouId).ConfigureAwait(false);
        return Ok(canCreate);
    }

    /// <summary>
    /// Check if organization unit can add more users
    /// </summary>
    [HttpGet("{ouId}/can-create-user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CanCreateUser(int ouId)
    {
        bool canCreate = await ouService.CanCreateUserAsync(ouId).ConfigureAwait(false);
        return Ok(canCreate);
    }
}
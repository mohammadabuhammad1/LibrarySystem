using LibrarySystem.API.Extensions;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Application.Dtos.Roles;
using LibrarySystem.Application.Dtos.Users;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LibrarySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AuthPolicy")]
public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    IBorrowRecordService borrowRecordService,
    IRoleService roleService) : BaseApiController(userManager)
{
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IBorrowRecordService _borrowRecordService = borrowRecordService;
    private readonly IRoleService _roleService = roleService;

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        ArgumentNullException.ThrowIfNull(loginDto);

        if (string.IsNullOrWhiteSpace(loginDto.Email))
            return BadRequest("Email is required");

        ApplicationUser? user = await UserManager!.FindByEmailAsync(loginDto.Email).ConfigureAwait(false);

        if (user == null)
            return Unauthorized("Invalid email or password");

        if (!user.IsActive)
            return Unauthorized("Account is deactivated. Please contact administrator.");

        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager
            .CheckPasswordSignInAsync(user, loginDto.Password ?? string.Empty, false)
            .ConfigureAwait(false);

        if (!result.Succeeded)
            return Unauthorized("Invalid email or password");

        IList<string> roles = await UserManager
            .GetRolesAsync(user)
            .ConfigureAwait(false);

        string token = await _tokenService
            .CreateToken(user)
            .ConfigureAwait(false);

        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        IEnumerable<BorrowRecordDto> activeBorrows = await _borrowRecordService.GetActiveBorrowsByUserAsync(user.Id).ConfigureAwait(false);
        IEnumerable<BorrowRecordDto> borrowHistory = await _borrowRecordService.GetUserBorrowHistoryAsync(user.Id).ConfigureAwait(false);
        IEnumerable<BorrowRecordDto> overdueBooks = await _borrowRecordService.GetOverdueBooksAsync().ConfigureAwait(false);
        int userOverdueBooks = overdueBooks.Count(b => b.UserId == user.Id);

        UserDto userDto = new()
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Token = token,
            MembershipDate = user.MembershipDate,
            IsActive = user.IsActive,
            TotalBooksBorrowed = borrowHistory.Count(),
            ActiveBorrows = activeBorrows.Count(),
            OverdueBooks = userOverdueBooks,
            TotalFines = borrowHistory.Sum(b => b.FineAmount ?? 0)
        };

        foreach (string role in roles)
        {
            userDto.Roles.Add(role);
        }

        return userDto;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        ArgumentNullException.ThrowIfNull(registerDto);

        if (string.IsNullOrWhiteSpace(registerDto.Email) ||
            string.IsNullOrWhiteSpace(registerDto.Password) ||
            string.IsNullOrWhiteSpace(registerDto.Name))
        {
            return BadRequest("Email, password, and name are required");
        }

        if (await UserManager!.FindByEmailAsync(registerDto.Email).ConfigureAwait(false) != null)
        {
            return BadRequest("Email address is already in use");
        }

        ApplicationUser user = new()
        {
            Name = registerDto.Name,
            Email = registerDto.Email,
            UserName = registerDto.Email,
            Phone = registerDto.Phone ?? string.Empty,
            MembershipDate = DateTime.UtcNow,
            IsActive = true
        };

        IdentityResult result = await UserManager.CreateAsync(user, registerDto.Password).ConfigureAwait(false);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await UserManager.AddToRoleAsync(user, UserRoles.Member).ConfigureAwait(false);

        IList<string> roles = await UserManager.GetRolesAsync(user).ConfigureAwait(false);
        string token = await _tokenService.CreateToken(user).ConfigureAwait(false);

        UserDto userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Token = token,
            MembershipDate = user.MembershipDate,
            IsActive = user.IsActive,
            TotalBooksBorrowed = 0,
            ActiveBorrows = 0,
            OverdueBooks = 0,
            TotalFines = 0
        };

        foreach (string role in roles)
        {
            userDto.Roles.Add(role);
        }

        return userDto;
    }

    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult> Logout()
    {
        Response.Cookies.Delete("access_token");
        await _signInManager.SignOutAsync().ConfigureAwait(false);
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("emailexists")]
    [AllowAnonymous]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
    {
        return await UserManager!.FindByEmailAsync(email).ConfigureAwait(false) != null;
    }

    [Authorize]
    [HttpGet("me")]
    [EnableRateLimiting("PerTenantPolicy")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        ApplicationUser? user = await GetCurrentUserAsync().ConfigureAwait(false);

        if (user == null)
            return Unauthorized("User not found");

        IList<string> roles = await UserManager!.GetRolesAsync(user).ConfigureAwait(false);

        IEnumerable<BorrowRecordDto> activeBorrows = await _borrowRecordService.GetActiveBorrowsByUserAsync(user.Id).ConfigureAwait(false);
        IEnumerable<BorrowRecordDto> borrowHistory = await _borrowRecordService.GetUserBorrowHistoryAsync(user.Id).ConfigureAwait(false);
        IEnumerable<BorrowRecordDto> overdueBooks = await _borrowRecordService.GetOverdueBooksAsync().ConfigureAwait(false);
        int userOverdueBooks = overdueBooks.Count(b => b.UserId == user.Id);

        UserDto userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Token = await _tokenService.CreateToken(user).ConfigureAwait(false),
            MembershipDate = user.MembershipDate,
            IsActive = user.IsActive,
            TotalBooksBorrowed = borrowHistory.Count(),
            ActiveBorrows = activeBorrows.Count(),
            OverdueBooks = userOverdueBooks,
            TotalFines = borrowHistory.Sum(b => b.FineAmount ?? 0)
        };

        foreach (string role in roles)
        {
            userDto.Roles.Add(role);
        }

        return userDto;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetUserProfile()
    {
        ApplicationUser? user = await GetCurrentUserAsync().ConfigureAwait(false);

        if (user == null)
            return NotFound("User not found");

        IList<string> roles = await UserManager!.GetRolesAsync(user).ConfigureAwait(false);

        UserDto userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Token = await _tokenService.CreateToken(user).ConfigureAwait(false),
            MembershipDate = user.MembershipDate,
            IsActive = user.IsActive
        };

        foreach (string role in roles)
        {
            userDto.Roles.Add(role);
        }

        return userDto;
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateUserProfile(UpdateProfileDto updateDto)
    {
        ArgumentNullException.ThrowIfNull(updateDto);

        ApplicationUser? user = await GetCurrentUserAsync().ConfigureAwait(false);

        if (user == null)
            return NotFound("User not found");

        user.Name = updateDto.Name ?? string.Empty;
        user.Phone = updateDto.Phone ?? string.Empty;

        IdentityResult result = await UserManager!.UpdateAsync(user).ConfigureAwait(false);

        if (!result.Succeeded)
            return BadRequest("Problem updating the user profile");

        IList<string> roles = await UserManager.GetRolesAsync(user).ConfigureAwait(false);

        UserDto userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Token = await _tokenService.CreateToken(user).ConfigureAwait(false),
            MembershipDate = user.MembershipDate,
            IsActive = user.IsActive
        };

        foreach (string role in roles)
        {
            userDto.Roles.Add(role);
        }

        return userDto;
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        ArgumentNullException.ThrowIfNull(changePasswordDto);

        ApplicationUser? user = await GetCurrentUserAsync().ConfigureAwait(false);

        if (user == null)
            return NotFound("User not found");

        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
        {
            return BadRequest("New passwords do not match");
        }

        IdentityResult result = await UserManager!.ChangePasswordAsync(
            user,
            changePasswordDto.CurrentPassword ?? string.Empty,
            changePasswordDto.NewPassword ?? string.Empty
        ).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            IEnumerable<string> errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        return Ok(new { message = "Password changed successfully" });
    }

    [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [HttpGet("users-with-borrows")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<List<UserWithBorrowsDto>>> GetUsersWithBorrows()
    {
        List<ApplicationUser> users = UserManager!.Users.ToList();
        List<UserWithBorrowsDto> result = new List<UserWithBorrowsDto>();

        foreach (ApplicationUser user in users)
        {
            IEnumerable<BorrowRecordDto> activeBorrows = await _borrowRecordService.GetActiveBorrowsByUserAsync(user.Id).ConfigureAwait(false);
            IEnumerable<BorrowRecordDto> borrowHistory = await _borrowRecordService.GetUserBorrowHistoryAsync(user.Id).ConfigureAwait(false);
            IList<string> roles = await UserManager.GetRolesAsync(user).ConfigureAwait(false);
            IEnumerable<BorrowRecordDto> overdueBooks = await _borrowRecordService.GetOverdueBooksAsync().ConfigureAwait(false);
            int userOverdueBooks = overdueBooks.Count(b => b.UserId == user.Id);

            UserWithBorrowsDto userWithBorrows = new UserWithBorrowsDto
            {
                UserId = user.Id,
                UserName = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Phone = user.Phone ?? string.Empty,
                IsActive = user.IsActive,
                ActiveBorrowsCount = activeBorrows.Count(),
                TotalBorrowsCount = borrowHistory.Count(),
                OverdueBooksCount = userOverdueBooks,
                HasOverdueBooks = userOverdueBooks > 0,
                MembershipDate = user.MembershipDate
            };

            foreach (string role in roles)
            {
                userWithBorrows.Roles.Add(role);
            }

            result.Add(userWithBorrows);
        }

        return result;
    }

    [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [HttpGet("user/{userId}/borrow-history")]
    public async Task<ActionResult<UserBorrowHistoryDto>> GetUserBorrowHistory(string userId)
    {
        ApplicationUser? user = await UserManager!.FindByIdAsync(userId).ConfigureAwait(false);
        if (user == null)
            return NotFound("User not found");

        IList<string> roles = await UserManager.GetRolesAsync(user).ConfigureAwait(false);
        IEnumerable<BorrowRecordDto> activeBorrows = await _borrowRecordService.GetActiveBorrowsByUserAsync(userId).ConfigureAwait(false);
        IEnumerable<BorrowRecordDto> borrowHistory = await _borrowRecordService.GetUserBorrowHistoryAsync(userId).ConfigureAwait(false);
        IEnumerable<BorrowRecordDto> overdueBooks = await _borrowRecordService.GetOverdueBooksAsync().ConfigureAwait(false);
        int userOverdueBooks = overdueBooks.Count(b => b.UserId == userId);

        UserBorrowHistoryDto userBorrowHistory = new UserBorrowHistoryDto
        {
            UserId = user.Id,
            UserName = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            OverdueBooksCount = userOverdueBooks,
            TotalFines = borrowHistory.Sum(b => b.FineAmount ?? 0)
        };

        foreach (string role in roles)
        {
            userBorrowHistory.Roles.Add(role);
        }

        foreach (BorrowRecordDto borrow in activeBorrows)
        {
            userBorrowHistory.ActiveBorrows.Add(borrow);
        }

        foreach (BorrowRecordDto borrow in borrowHistory)
        {
            userBorrowHistory.BorrowHistory.Add(borrow);
        }

        return userBorrowHistory;
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [HttpGet("all-users")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<List<AdminUserDto>>> GetAllUsers()
    {
        List<ApplicationUser> users = UserManager!.Users.ToList();
        List<AdminUserDto> userDtos = new List<AdminUserDto>();

        foreach (ApplicationUser user in users)
        {
            IList<string> roles = await UserManager.GetRolesAsync(user).ConfigureAwait(false);
            IEnumerable<BorrowRecordDto> activeBorrows = await _borrowRecordService.GetActiveBorrowsByUserAsync(user.Id).ConfigureAwait(false);
            IEnumerable<BorrowRecordDto> borrowHistory = await _borrowRecordService.GetUserBorrowHistoryAsync(user.Id).ConfigureAwait(false);

            AdminUserDto adminUser = new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Phone = user.Phone ?? string.Empty,
                MembershipDate = user.MembershipDate,
                IsActive = user.IsActive,
                TotalBorrows = borrowHistory.Count(),
                ActiveBorrows = activeBorrows.Count(),
                LastLogin = DateTime.UtcNow
            };

            foreach (string role in roles)
            {
                adminUser.Roles.Add(role);
            }

            userDtos.Add(adminUser);
        }

        return userDtos;
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<AdminUserDto>> GetUserById(string userId)
    {
        ApplicationUser? user = await UserManager!.FindByIdAsync(userId).ConfigureAwait(false);
        if (user == null)
            return NotFound("User not found");

        IList<string> roles = await UserManager.GetRolesAsync(user).ConfigureAwait(false);
        IEnumerable<BorrowRecordDto> activeBorrows = await _borrowRecordService.GetActiveBorrowsByUserAsync(userId).ConfigureAwait(false);
        IEnumerable<BorrowRecordDto> borrowHistory = await _borrowRecordService.GetUserBorrowHistoryAsync(userId).ConfigureAwait(false);

        AdminUserDto adminUser = new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            MembershipDate = user.MembershipDate,
            IsActive = user.IsActive,
            TotalBorrows = borrowHistory.Count(),
            ActiveBorrows = activeBorrows.Count(),
            LastLogin = DateTime.UtcNow
        };

        foreach (string role in roles)
        {
            adminUser.Roles.Add(role);
        }

        return adminUser;
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [HttpPost("assign-role")]
    [EnableRateLimiting("ApiPolicy")] // Sensitive operation
    public async Task<ActionResult> AssignRoleToUser([FromBody] AssignRoleDto assignRoleDto)
    {
        ArgumentNullException.ThrowIfNull(assignRoleDto);

        try
        {
            IdentityResult result = await _roleService.AssignRoleToUserAsync(assignRoleDto.UserId, assignRoleDto.RoleName).ConfigureAwait(false);

            if (result.Succeeded)
                return Ok(new { message = $"Role '{assignRoleDto.RoleName}' assigned successfully" });

            return BadRequest(result.Errors);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [HttpPost("remove-role")]
    public async Task<ActionResult> RemoveRoleFromUser([FromBody] AssignRoleDto removeRoleDto)
    {
        ArgumentNullException.ThrowIfNull(removeRoleDto);

        try
        {
            IdentityResult result = await _roleService.RemoveRoleFromUserAsync(removeRoleDto.UserId, removeRoleDto.RoleName).ConfigureAwait(false);

            if (result.Succeeded)
                return Ok(new { message = $"Role '{removeRoleDto.RoleName}' removed successfully" });

            return BadRequest(result.Errors);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [HttpPut("deactivate/{userId}")]
    public async Task<ActionResult> DeactivateUser(string userId)
    {
        ApplicationUser? user = await UserManager!.FindByIdAsync(userId).ConfigureAwait(false);
        if (user == null)
            return NotFound("User not found");

        user.IsActive = false;
        IdentityResult result = await UserManager.UpdateAsync(user).ConfigureAwait(false);

        if (result.Succeeded)
            return Ok(new { message = "User deactivated successfully" });

        return BadRequest("Problem deactivating user");
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SuperAdmin}")]
    [HttpPut("activate/{userId}")]
    public async Task<ActionResult> ActivateUser(string userId)
    {
        ApplicationUser? user = await UserManager!.FindByIdAsync(userId).ConfigureAwait(false);
        if (user == null)
            return NotFound("User not found");

        user.IsActive = true;
        IdentityResult result = await UserManager.UpdateAsync(user).ConfigureAwait(false);

        if (result.Succeeded)
            return Ok(new { message = "User activated successfully" });

        return BadRequest("Problem activating user");
    }

    [Authorize(Roles = UserRoles.SuperAdmin)]
    [HttpDelete("user/{userId}")]
    public async Task<ActionResult> DeleteUser(string userId)
    {
        ApplicationUser? currentUser = await UserManager!.FindByEmailFromClaimsPrincipal(User).ConfigureAwait(false);
        if (currentUser?.Id == userId)
            return BadRequest("Cannot delete your own account");

        ApplicationUser? user = await UserManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user == null)
            return NotFound("User not found");

        IEnumerable<BorrowRecordDto> activeBorrows = await _borrowRecordService.GetActiveBorrowsByUserAsync(userId).ConfigureAwait(false);
        if (activeBorrows.Any())
            return BadRequest("Cannot delete user with active book borrows");

        IdentityResult result = await UserManager.DeleteAsync(user).ConfigureAwait(false);

        if (result.Succeeded)
            return Ok(new { message = "User deleted successfully" });

        return BadRequest("Problem deleting user");
    }

    [Authorize(Roles = UserRoles.SuperAdmin)]
    [HttpPost("create-librarian")]
    [EnableRateLimiting("ApiPolicy")] 
    public async Task<ActionResult<UserDto>> CreateLibrarian(RegisterDto registerDto)
    {
        ArgumentNullException.ThrowIfNull(registerDto);

        if (await UserManager!.FindByEmailAsync(registerDto.Email).ConfigureAwait(false) != null)
        {
            return BadRequest("Email address is already in use");
        }

        ApplicationUser user = new ApplicationUser
        {
            Name = registerDto.Name,
            Email = registerDto.Email,
            UserName = registerDto.Email,
            Phone = registerDto.Phone ?? string.Empty,
            MembershipDate = DateTime.UtcNow,
            IsActive = true
        };

        IdentityResult result = await UserManager.CreateAsync(user, registerDto.Password).ConfigureAwait(false);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await UserManager.AddToRoleAsync(user, UserRoles.Librarian).ConfigureAwait(false);

        IList<string> roles = await UserManager.GetRolesAsync(user).ConfigureAwait(false);
        string token = await _tokenService.CreateToken(user).ConfigureAwait(false);

        UserDto userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Token = token,
            MembershipDate = user.MembershipDate,
            IsActive = user.IsActive,
            TotalBooksBorrowed = 0,
            ActiveBorrows = 0,
            OverdueBooks = 0,
            TotalFines = 0
        };

        foreach (string role in roles)
        {
            userDto.Roles.Add(role);
        }

        return userDto;
    }

    [Authorize(Roles = UserRoles.SuperAdmin)]
    [HttpPost("create-admin")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<UserDto>> CreateAdmin(RegisterDto registerDto)
    {
        ArgumentNullException.ThrowIfNull(registerDto);

        if (await UserManager!.FindByEmailAsync(registerDto.Email).ConfigureAwait(false) != null)
        {
            return BadRequest("Email address is already in use");
        }

        ApplicationUser user = new ApplicationUser
        {
            Name = registerDto.Name,
            Email = registerDto.Email,
            UserName = registerDto.Email,
            Phone = registerDto.Phone ?? string.Empty,
            MembershipDate = DateTime.UtcNow,
            IsActive = true
        };

        IdentityResult result = await UserManager.CreateAsync(user, registerDto.Password).ConfigureAwait(false);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await UserManager.AddToRoleAsync(user, UserRoles.Admin).ConfigureAwait(false);

        IList<string> roles = await UserManager.GetRolesAsync(user).ConfigureAwait(false);
        string token = await _tokenService.CreateToken(user).ConfigureAwait(false);

        UserDto userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Token = token,
            MembershipDate = user.MembershipDate,
            IsActive = user.IsActive,
            TotalBooksBorrowed = 0,
            ActiveBorrows = 0,
            OverdueBooks = 0,
            TotalFines = 0
        };

        foreach (string role in roles)
        {
            userDto.Roles.Add(role);
        }

        return userDto;
    }

    [Authorize(Roles = UserRoles.SuperAdmin)]
    [HttpGet("roles")]
    public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
    {
        List<RoleDto> roles = await _roleService.GetAllRolesAsync().ConfigureAwait(false);
        return roles;
    }

    [Authorize(Roles = UserRoles.SuperAdmin)]
    [HttpGet("role/{roleName}/users")]
    public async Task<ActionResult<List<UserRoleDto>>> GetUsersInRole(string roleName)
    {
        if (!await _roleService.RoleExistsAsync(roleName).ConfigureAwait(false))
            return NotFound($"Role '{roleName}' not found");

        List<UserRoleDto> users = await _roleService.GetUsersInRoleAsync(roleName).ConfigureAwait(false);
        return users;
    }
}
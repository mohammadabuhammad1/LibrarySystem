using AutoMapper;
using LibrarySystem.Application.Dtos.Users;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LibrarySystem.Application.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    IMapper mapper,
    ITokenService tokenService) : IUserService
{
    public async Task<IdentityResult> RegisterAsync(string email, string password, string name, string phone)
    {
        ApplicationUser user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Name = name,
            Phone = phone,
            MembershipDate = DateTime.UtcNow,
            IsActive = true
        };

        return await userManager.CreateAsync(user, password).ConfigureAwait(false);
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (user == null || !await userManager.CheckPasswordAsync(user, password).ConfigureAwait(false))
        {
            return null;
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("User account is deactivated.");
        }

        return await tokenService.CreateToken(user).ConfigureAwait(false);
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        return mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        return mapper.Map<UserDto>(user);
    }

    public async Task<IdentityResult> UpdateUserAsync(string userId, string name, string phone)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }

        user.Name = name;
        user.Phone = phone;

        return await userManager.UpdateAsync(user).ConfigureAwait(false);
    }

    public async Task<IdentityResult> DeactivateUserAsync(string userId)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }

        user.IsActive = false;
        return await userManager.UpdateAsync(user).ConfigureAwait(false);
    }

    public async Task<IdentityResult> ActivateUserAsync(string userId)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }

        user.IsActive = true;
        return await userManager.UpdateAsync(user).ConfigureAwait(false);
    }
}
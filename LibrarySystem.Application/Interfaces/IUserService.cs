using LibrarySystem.Application.Dtos.Users;
using Microsoft.AspNetCore.Identity;

namespace LibrarySystem.Application.Interfaces;

public interface IUserService
{
    Task<IdentityResult> RegisterAsync(string email, string password, string name, string phone);
    Task<string?> LoginAsync(string email, string password);

    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto?> GetUserByEmailAsync(string email);

    Task<IdentityResult> UpdateUserAsync(string userId, string name, string phone);
    Task<IdentityResult> DeactivateUserAsync(string userId);
    Task<IdentityResult> ActivateUserAsync(string userId);
}
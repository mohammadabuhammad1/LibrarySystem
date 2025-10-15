﻿using LibrarySystem.Application.Dtos.Roles;
using Microsoft.AspNetCore.Identity;

namespace LibrarySystem.Application.Interfaces;

public interface IRoleService
{
    Task<IdentityResult> CreateRoleAsync(string roleName);
    Task<bool> RoleExistsAsync(string roleName);
    Task<IdentityResult> AssignRoleToUserAsync(string userId, string roleName);
    Task<IdentityResult> RemoveRoleFromUserAsync(string userId, string roleName);
    Task<IList<string>> GetUserRolesAsync(string userId);
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<List<UserRoleDto>> GetUsersInRoleAsync(string roleName);
    Task <IdentityResult> DeleteRoleAsync(string roleName);
}

using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Interfaces;

public interface ITokenService
{
    Task<string> CreateToken(ApplicationUser user);
    Task<string?> GetUserIdFromToken(string token);
}
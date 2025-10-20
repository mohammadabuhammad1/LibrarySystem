// Infrastructure/Constants/UserRoles.cs
namespace LibrarySystem.Infrastructure.Constants;

public static class UserRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Librarian = "Librarian";
    public const string Member = "Member";


    public static readonly string[] AllRoles =
    {
        SuperAdmin, Admin, Librarian, Member,
    };

    // Helper methods to check role hierarchies
    public static bool IsGlobalAdmin(string role) => role == SuperAdmin || role == Admin;

}
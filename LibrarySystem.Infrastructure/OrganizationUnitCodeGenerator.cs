using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using System.Globalization;

namespace LibrarySystem.Infrastructure;

/// <summary>
/// Generates hierarchical codes for Organization Units.
/// Format: 0001.0001.0001 (4 digits per level)
/// </summary>
public class OrganizationUnitCodeGenerator(IUnitOfWork unitOfWork)
{
    private const int CodeUnitLength = 4; // Each segment is 4 digits

    /// <summary>
    /// Generate the next available code for a new Organization Unit.
    /// </summary>
    /// <param name="parentId">Parent OU ID, or null for root-level OUs.</param>
    public async Task<string> GenerateNextCodeAsync(int? parentId = null)
    {
        if (parentId == null)
        {
            // Root level - find next root code
            return await GenerateRootCodeAsync().ConfigureAwait(false);
        }

        // Child level - append to parent code
        return await GenerateChildCodeAsync(parentId.Value).ConfigureAwait(false);
    }

    private async Task<string> GenerateRootCodeAsync()
    {
        IEnumerable<OrganizationUnit> rootOUs = await unitOfWork.OrganizationUnits
            .GetRootOrganizationUnitsAsync()
            .ConfigureAwait(false);

        if (!rootOUs.Any())
        {
            return FormatCode(1); // First root: 0001
        }

        // Get the highest root code
        int maxCode = rootOUs
            .Select(ou => ParseFirstSegment(ou.Code))
            .Max();

        return FormatCode(maxCode + 1);
    }

    private async Task<string> GenerateChildCodeAsync(int parentId)
    {
        OrganizationUnit? parent = await unitOfWork.OrganizationUnits
            .GetByIdAsync(parentId)
            .ConfigureAwait(false);

        if (parent == null)
        {
            throw new InvalidOperationException($"Parent Organization Unit with ID {parentId} not found.");
        }

        IEnumerable<OrganizationUnit> children = await unitOfWork.OrganizationUnits
            .GetChildrenAsync(parentId)
            .ConfigureAwait(false);

        if (!children.Any())
        {
            // First child
            return parent.Code + "." + FormatCode(1); // e.g., 0001.0001
        }

        // Get the highest child code at this level
        var childCodes = children
            .Select(ou => ParseLastSegment(ou.Code))
            .ToList();

        int maxChildCode = childCodes.Max();
        return parent.Code + "." + FormatCode(maxChildCode + 1);
    }

    /// <summary>
    /// Validate that a code follows the correct format.
    /// </summary>
    /// <param name="code">The code to validate.</param>
    public static bool IsValidCode(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        string[] segments = code.Split('.');

        foreach (string segment in segments)
        {
            if (segment.Length != CodeUnitLength)
            {
                return false;
            }

            if (!int.TryParse(segment, NumberStyles.None, CultureInfo.InvariantCulture, out int number))
            {
                return false;
            }

            if (number < 1)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get the parent code from a child code.
    /// </summary>
    /// <param name="code">The full child code.</param>
    /// <returns>The parent code, or null if root.</returns>
    public static string? GetParentCode(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        int lastDotIndex = code.LastIndexOf('.');
        if (lastDotIndex == -1)
        {
            return null; // Root has no parent
        }

        return code[..lastDotIndex];
    }

    /// <summary>
    /// Get the level (depth) of an OU based on its code.
    /// </summary>
    /// <param name="code">The hierarchical code.</param>
    public static int GetLevel(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        return code.Split('.').Length;
    }

    /// <summary>
    /// Format an integer as a 4-digit segment (e.g., 1 → 0001).
    /// </summary>
    private static string FormatCode(int number)
    {
        return number.ToString($"D{CodeUnitLength}", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parse the first segment (root part) of a code.
    /// </summary>
    private static int ParseFirstSegment(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        string firstSegment = code.Split('.')[0];
        return int.Parse(firstSegment, NumberStyles.None, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parse the last segment of a hierarchical code.
    /// </summary>
    private static int ParseLastSegment(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        string[] segments = code.Split('.');
        return int.Parse(segments[^1], NumberStyles.None, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Get the full code path for an existing Organization Unit.
    /// </summary>
    /// <param name="ouId">The Organization Unit ID.</param>
    public async Task<string> GetFullCodePathAsync(int ouId)
    {
        OrganizationUnit? ou = await unitOfWork.OrganizationUnits
            .GetByIdAsync(ouId)
            .ConfigureAwait(false);

        return ou?.Code ?? string.Empty;
    }

    /// <summary>
    /// Check if a code already exists in the system.
    /// </summary>
    /// <param name="code">The code to check.</param>
    public async Task<bool> CodeExistsAsync(string code)
    {
        return await unitOfWork.OrganizationUnits
            .CodeExistsAsync(code)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Get all descendant codes for a given OU code.
    /// </summary>
    /// <param name="parentCode">The parent OU code.</param>
    public async Task<IEnumerable<string>> GetDescendantCodesAsync(string parentCode)
    {
        // Find the OU by code first
        OrganizationUnit? parent = await unitOfWork.OrganizationUnits
            .GetByCodeAsync(parentCode)
            .ConfigureAwait(false);

        if (parent == null)
        {
            return Enumerable.Empty<string>();
        }

        // Get all descendants
        IEnumerable<OrganizationUnit> descendants = await unitOfWork.OrganizationUnits
            .GetDescendantsAsync(parent.Id)
            .ConfigureAwait(false);

        return descendants.Select(ou => ou.Code);
    }
}
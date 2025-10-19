// Infrastructure/Services/OrganizationUnitCodeGenerator.cs
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using System.Globalization;

namespace LibrarySystem.Infrastructure.Services;

public class OrganizationUnitCodeGenerator(IUnitOfWork unitOfWork) : IOrganizationUnitCodeGenerator
{
    private const int CodeUnitLength = 4;

    public async Task<string> GenerateNextCodeAsync(int? parentId = null)
    {
        if (parentId == null)
        {
            return await GenerateRootCodeAsync().ConfigureAwait(false);
        }
        return await GenerateChildCodeAsync(parentId.Value).ConfigureAwait(false);
    }

    private async Task<string> GenerateRootCodeAsync()
    {
        IEnumerable<OrganizationUnit> rootOUs = await unitOfWork.OrganizationUnits
            .GetRootOrganizationUnitsAsync()
            .ConfigureAwait(false);

        if (!rootOUs.Any())
        {
            return FormatCode(1);
        }

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
            return parent.Code + "." + FormatCode(1);
        }

        var childCodes = children
            .Select(ou => ParseLastSegment(ou.Code))
            .ToList();

        int maxChildCode = childCodes.Max();
        return parent.Code + "." + FormatCode(maxChildCode + 1);
    }

    public async Task<string> GetFullCodePathAsync(int ouId)
    {
        OrganizationUnit? ou = await unitOfWork.OrganizationUnits
            .GetByIdAsync(ouId)
            .ConfigureAwait(false);

        return ou?.Code ?? string.Empty;
    }

    public async Task<bool> CodeExistsAsync(string code)
    {
        return await unitOfWork.OrganizationUnits
            .CodeExistsAsync(code)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<string>> GetDescendantCodesAsync(string parentCode)
    {
        OrganizationUnit? parent = await unitOfWork.OrganizationUnits
            .GetByCodeAsync(parentCode)
            .ConfigureAwait(false);

        if (parent == null)
        {
            return Enumerable.Empty<string>();
        }

        IEnumerable<OrganizationUnit> descendants = await unitOfWork.OrganizationUnits
            .GetDescendantsAsync(parent.Id)
            .ConfigureAwait(false);

        return descendants.Select(ou => ou.Code);
    }

    // Static helper methods
    public static bool IsValidCode(string code)
    {
        // ... keep the static validation methods
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

    public static string? GetParentCode(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        int lastDotIndex = code.LastIndexOf('.');
        if (lastDotIndex == -1)
        {
            return null;
        }

        return code[..lastDotIndex];
    }

    public static int GetLevel(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        return code.Split('.').Length;
    }

    private static string FormatCode(int number)
    {
        return number.ToString($"D{CodeUnitLength}", CultureInfo.InvariantCulture);
    }

    private static int ParseFirstSegment(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        string firstSegment = code.Split('.')[0];
        return int.Parse(firstSegment, NumberStyles.None, CultureInfo.InvariantCulture);
    }

    private static int ParseLastSegment(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        string[] segments = code.Split('.');
        return int.Parse(segments[^1], NumberStyles.None, CultureInfo.InvariantCulture);
    }
}
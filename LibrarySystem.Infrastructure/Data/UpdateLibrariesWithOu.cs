using LibrarySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Data;

public static class UpdateLibrariesWithOu
{
    public static async Task MigrateExistingLibrariesAsync(LibraryDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Get the first (or default) organization unit
        OrganizationUnit? defaultOu = await context.OrganizationUnits
            .FirstOrDefaultAsync(ou => ou.ParentId == null).ConfigureAwait(false);

        if (defaultOu == null)
        {
            return;
        }

        // Update all libraries without an OrganizationUnitId or with default value
        List<Library> librariesWithoutOu = await context.Libraries
            .Where(l => l.OrganizationUnitId == 0)
            .ToListAsync().ConfigureAwait(false);

        foreach (Library library in librariesWithoutOu)
        {
            library.AssignOrganizationUnit(defaultOu.Id);
        }

        if (librariesWithoutOu.Count > 0)
        {
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

    }
}
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Repositories;

public class LibraryRepository(LibraryDbContext context) : GenericRepository<Library>(context), ILibraryRepository
{
    public async Task<Library?> GetByNameAsync(string name)
    {
        return await context.Set<Library>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .ConfigureAwait(false);

        //  Uses StringComparison.OrdinalIgnoreCase for case-insensitive comparison
        //  No string allocation (unlike ToLower/ToUpper which create new strings)
        //  Culture-invariant and safe for all locales
        //  Better performance - direct comparison without temporary strings
        //  EF Core translates this efficiently to SQL

    }

    public async Task<IEnumerable<Library>> GetLibrariesWithBooksAsync()
    {
        return await context.Set<Library>()
            .AsNoTracking()
            .Include(l => l.Books)
            .ToListAsync().ConfigureAwait(false);
    }
}
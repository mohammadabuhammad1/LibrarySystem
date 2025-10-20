using LibrarySystem.Application.Interfaces;
using LibrarySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Data;

public class TenantAwareRepository<T>(LibraryDbContext context, ITenantProvider tenantProvider) where T : class
{
    protected LibraryDbContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));
    protected ITenantProvider TenantProvider { get; } = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
    protected DbSet<T> DbSet { get; } = context.Set<T>();

    protected virtual IQueryable<T> ApplyTenantFilter(IQueryable<T> query)
    {
        int? tenantId = TenantProvider.GetCurrentTenantId();

        if (!tenantId.HasValue)
        {
            return query; // No tenant context, return all data
        }

        // Apply tenant-specific filtering based on entity type
        return typeof(T) switch
        {
            Type libraryType when libraryType == typeof(Library) => ApplyLibraryTenantFilter(query, tenantId.Value),
            Type bookType when bookType == typeof(Book) => ApplyBookTenantFilter(query, tenantId.Value),
            Type borrowRecordType when borrowRecordType == typeof(BorrowRecord) => ApplyBorrowRecordTenantFilter(query, tenantId.Value),
            _ => query // No tenant filtering for this entity type
        };
    }

    private static IQueryable<T> ApplyLibraryTenantFilter(IQueryable<T> query, int tenantId)
    {
        return query.Cast<Library>()
            .Where(l => l.OrganizationUnit != null &&
                       (l.OrganizationUnit.ParentId == tenantId || l.OrganizationUnitId == tenantId))
            .Cast<T>();
    }

    private static IQueryable<T> ApplyBookTenantFilter(IQueryable<T> query, int tenantId)
    {
        return query.Cast<Book>()
            .Where(b => b.Library != null && b.Library.OrganizationUnit != null &&
                       (b.Library.OrganizationUnit.ParentId == tenantId || b.Library.OrganizationUnitId == tenantId))
            .Cast<T>();
    }

    private static IQueryable<T> ApplyBorrowRecordTenantFilter(IQueryable<T> query, int tenantId)
    {
        return query.Cast<BorrowRecord>()
            .Where(br => br.Book != null && br.Book.Library != null && br.Book.Library.OrganizationUnit != null &&
                        (br.Book.Library.OrganizationUnit.ParentId == tenantId || br.Book.Library.OrganizationUnitId == tenantId))
            .Cast<T>();
    }

    // Helper methods for common operations
    public IQueryable<T> GetAll()
    {
        return ApplyTenantFilter(DbSet.AsQueryable());
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        IQueryable<T> query = ApplyTenantFilter(DbSet.AsQueryable());
        return await query.FirstOrDefaultAsync(GetIdPredicate(id)).ConfigureAwait(false);
    }

    // You'll need to implement this method based on your entity structure
    private static System.Linq.Expressions.Expression<Func<T, bool>> GetIdPredicate(int id)
    {
        // This is a simplified version - you'll need to adjust based on your entity's ID property
        System.Linq.Expressions.ParameterExpression parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "entity");
        System.Linq.Expressions.MemberExpression property = System.Linq.Expressions.Expression.Property(parameter, "Id");
        System.Linq.Expressions.ConstantExpression constant = System.Linq.Expressions.Expression.Constant(id);
        System.Linq.Expressions.BinaryExpression equal = System.Linq.Expressions.Expression.Equal(property, constant);

        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(equal, parameter);
    }
}
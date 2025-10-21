using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Repositories;

public class GenericRepository<T>(LibraryDbContext context) : IGenericRepository<T> where T : BaseEntity
{

    public async Task<T?> GetByIdAsync(int id)
    {
        return await context.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await context.Set<T>()
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<T> AddAsync(T entity)// Changed return type to Task if SaveChangesAsync is moved
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.CreatedAt = DateTime.UtcNow;
        await context.Set<T>().AddAsync(entity).ConfigureAwait(false);
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.UpdatedAt = DateTime.UtcNow;
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync().ConfigureAwait(false);//review
    }

    public async Task DeleteAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        context.Set<T>().Remove(entity);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await context.Set<T>().AnyAsync(e => e.Id == id).ConfigureAwait(false);
    }

    public async Task<T?> GetByIdTrackedAsync(int id)
    {
        return await context.Set<T>().FindAsync(id).ConfigureAwait(false);
    }
}
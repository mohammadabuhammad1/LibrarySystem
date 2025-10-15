using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Domain.Interfaces;

public interface ILibraryRepository : IGenericRepository<Library>
{
    Task<Library?> GetByNameAsync(string name);
    Task<IEnumerable<Library>> GetLibrariesWithBooksAsync();
}
using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.Entities;

public class Library : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ICollection<Book> Books { get; private set; } = [];

    private Library() { }

    public static Library Create(string name, string location, string? description = null, int organizationUnitId = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location is required", nameof(location));

        var library = new Library
        {
            Name = name.Trim(),
            Location = location.Trim(),
            Description = description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        return library;
    }

    public void Update(string name, string location, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name), "Name is required");

        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentNullException(nameof(location), "Location is required");

        Name = name.Trim();
        Location = location.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }


}
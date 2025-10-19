using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Domain.Entities;


public class Library : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }


    public int OrganizationUnitId { get; set; }
    public virtual OrganizationUnit OrganizationUnit { get; set; } = null!;


    // Navigation properties
    public virtual ICollection<Book> Books { get;  } = [];
}

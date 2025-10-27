using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.Commands.Books;

public class UpdateBookCommand : BaseCommand
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public int? PublishedYear { get; set; }  
    public int? TotalCopies { get; set; }    
    public string? Description { get; set; }
    public string? Genre { get; set; }
}
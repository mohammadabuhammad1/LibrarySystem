namespace LibrarySystem.Domain.Queries;

public class GetBookByIsbnQuery(string isbn) : BaseQuery
{
    public string ISBN { get; } = isbn ?? throw new ArgumentNullException(nameof(isbn));
}

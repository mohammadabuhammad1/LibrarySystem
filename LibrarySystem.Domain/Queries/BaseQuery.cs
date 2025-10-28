namespace LibrarySystem.Domain.Queries;

public abstract class BaseQuery
{
    public DateTime QueryDate { get; } = DateTime.UtcNow;
    public string? QueryBy { get; set; }
    public string? CorrelationId { get; set; }

    protected BaseQuery()
    {
        CorrelationId = Guid.NewGuid().ToString();
    }
}
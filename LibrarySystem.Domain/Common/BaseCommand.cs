namespace LibrarySystem.Domain.Common;

public abstract class BaseCommand
{
    public DateTime CommandDate { get; } = DateTime.UtcNow;
    public string? CommandBy { get; set; }  
    public string? CorrelationId { get; set; }  

    protected BaseCommand()
    {
        CorrelationId = Guid.NewGuid().ToString();
    }
}
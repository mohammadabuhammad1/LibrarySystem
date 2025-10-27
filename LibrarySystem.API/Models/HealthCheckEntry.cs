namespace LibrarySystem.API.Models;

public class HealthCheckEntry
{
    public required string Status { get; init; }
    public string? Description { get; init; }
    public TimeSpan Duration { get; init; }
    public IReadOnlyDictionary<string, object>? Data { get; init; }
    public string? Exception { get; init; }
}
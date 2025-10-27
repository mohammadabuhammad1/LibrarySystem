namespace LibrarySystem.API.Models;

public class HealthCheckResponse
{
    public required string Status { get; init; }
    public TimeSpan Duration { get; init; }
    public required Dictionary<string, HealthCheckEntry> Checks { get; init; }
    public DateTime Timestamp { get; init; }
}
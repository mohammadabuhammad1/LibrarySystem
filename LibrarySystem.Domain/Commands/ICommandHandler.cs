using LibrarySystem.Domain.Common;
using System.Collections.ObjectModel;

namespace LibrarySystem.Domain.Interfaces;

public interface ICommandHandler<TCommand> where TCommand : BaseCommand
{
    Task<CommandResult> HandleAsync(TCommand command);
}

public interface ICommandDispatcher
{
    Task<CommandResult> DispatchAsync<TCommand>(TCommand command) where TCommand : BaseCommand;
}

public class CommandResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public object? Data { get; init; }
    public ReadOnlyCollection<string> Errors { get; }

    private CommandResult(bool success, string? message = null, object? data = null, IEnumerable<string>? errors = null)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = new ReadOnlyCollection<string>(errors?.ToList() ?? new List<string>());
    }

    public static CommandResult Ok(object? data = null, string? message = null)
        => new(true, message, data);

    public static CommandResult Fail(string message, IEnumerable<string>? errors = null)
        => new(false, message, errors: errors);

    public static CommandResult ValidationFailed(IEnumerable<string> errors)
        => new(false, "Validation failed", errors: errors);
}

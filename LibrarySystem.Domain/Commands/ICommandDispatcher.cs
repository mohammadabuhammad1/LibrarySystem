using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.Commands;

public interface ICommandDispatcher
{
    Task<CommandResult> DispatchAsync<TCommand>(TCommand command) where TCommand : BaseCommand;
}

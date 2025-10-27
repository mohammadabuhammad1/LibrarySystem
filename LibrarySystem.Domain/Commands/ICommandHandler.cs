using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.Commands;

public interface ICommandHandler<TCommand> where TCommand : BaseCommand
{
    Task<CommandResult> HandleAsync(TCommand command);
}

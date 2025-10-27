using LibrarySystem.Domain.Commands;
using LibrarySystem.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem.Application.Commands.Handlers;

public class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public async Task<CommandResult> DispatchAsync<TCommand>(TCommand command) where TCommand : BaseCommand
    {
        ICommandHandler<TCommand>? handler = serviceProvider.GetService<ICommandHandler<TCommand>>();
        if (handler == null)
            throw new InvalidOperationException($"No command handler registered for {typeof(TCommand).Name}");

        return await handler.HandleAsync(command).ConfigureAwait(false);
    }
}
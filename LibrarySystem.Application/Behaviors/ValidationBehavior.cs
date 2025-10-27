using FluentValidation;
using LibrarySystem.Domain.Commands;
using LibrarySystem.Domain.Common;
using FluentValidation.Results;

namespace LibrarySystem.Application.Behaviors;

public class ValidationBehavior<TCommand>(
    ICommandHandler<TCommand> inner,
    IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand>
    where TCommand : BaseCommand
{
    public async Task<CommandResult> HandleAsync(TCommand command)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TCommand>(command);

            ValidationResult[] validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context)))
                .ConfigureAwait(false); 

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                var errors = string.Join("; ", failures.Select(f => f.ErrorMessage));
                return CommandResult.Fail($"Validation failed: {errors}");
            }
        }

        return await inner.HandleAsync(command).ConfigureAwait(false);
    }
}
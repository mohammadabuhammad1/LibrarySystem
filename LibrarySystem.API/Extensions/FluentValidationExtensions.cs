using FluentValidation;
using LibrarySystem.Application.Validators.Commands;
using LibrarySystem.Domain.Commands.Books;

namespace LibrarySystem.API.Extensions;

public static class FluentValidationExtensions
{
    /// <summary>
    /// Registers FluentValidation services and validators.
    /// </summary>
    public static IServiceCollection AddFluentValidationServices(
        this IServiceCollection services)
    {
        // Register all validators from the Application assembly
        services.AddValidatorsFromAssembly(
            typeof(CreateBookCommandValidator).Assembly,
            ServiceLifetime.Scoped);

        // Optional: Add custom validation configuration
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

        return services;
    }

    /// <summary>
    /// Registers specific validators for command handlers.
    /// Use this for explicit validator registration per command.
    /// </summary>
    public static IServiceCollection AddCommandValidators(
        this IServiceCollection services)
    {
        // Book Command Validators
        services.AddScoped<IValidator<CreateBookCommand>, CreateBookCommandValidator>();
        services.AddScoped<IValidator<UpdateBookCommand>, UpdateBookCommandValidator>();
        services.AddScoped<IValidator<DeleteBookCommand>, DeleteBookCommandValidator>();
        services.AddScoped<IValidator<BorrowBookCommand>, BorrowBookCommandValidator>();
        services.AddScoped<IValidator<ReturnBookCommand>, ReturnBookCommandValidator>();
        services.AddScoped<IValidator<UpdateBookCopiesCommand>, UpdateBookCopiesCommandValidator>();

        return services;
    }
}
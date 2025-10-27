using LibrarySystem.Application.Commands.Handlers;
using LibrarySystem.Application.Commands.Handlers.Books;
using LibrarySystem.Domain.Commands;
using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Infrastructure.Data;
using LibrarySystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<LibraryDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IBorrowRecordRepository, BorrowRecordRepository>();
        services.AddScoped<ILibraryRepository, LibraryRepository>();

        // Command Handlers
        services.AddScoped<ICommandHandler<CreateBookCommand>, CreateBookCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateBookCommand>, UpdateBookCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteBookCommand>, DeleteBookCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateBookCopiesCommand>, UpdateBookCopiesCommandHandler>();
        services.AddScoped<ICommandHandler<BorrowBookCommand>, BorrowBookCommandHandler>();
        services.AddScoped<ICommandHandler<ReturnBookCommand>, ReturnBookCommandHandler>();

        // Command Dispatcher
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
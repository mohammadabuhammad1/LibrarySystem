using FluentValidation;
using FluentValidation.AspNetCore; 
using LibrarySystem.API.Services;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Application.Services;
using LibrarySystem.Application.Validators.Commands;
using LibrarySystem.Infrastructure;
using LibrarySystem.Infrastructure.Data;


namespace LibrarySystem.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Basic services
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();

        //// Add FluentValidation BEFORE other configurations
        services.AddFluentValidationAutoValidation();

        services.AddFluentValidationClientsideAdapters();

        services.AddValidatorsFromAssemblyContaining<CreateBookCommandValidator>();

        // Configure CORS
        services.AddCorsConfiguration();

        // Configure Rate Limiting
        services.AddRateLimitingConfiguration();

        // Configure Health Checks
        services.AddComprehensiveHealthChecks();

        // Configure Infrastructure (Database, Repositories, etc.)
        services.AddInfrastructure(configuration);

        // Configure Identity
        services.AddIdentityConfiguration();

        // Configure Authentication & JWT
        services.AddAuthenticationConfiguration(configuration);

        // Configure Swagger
        services.AddSwaggerConfiguration();

        // Register Application Layer (includes validators)
        //services.AddApplication()

        // Register Application Services
        services.AddApplicationLayerServices();

        // Register AutoMapper
        services.AddAutoMapper(typeof(BookDto).Assembly);

        // Register Data Seeding Services
        services.AddScoped<DataSeeder>();
        services.AddScoped<DatabaseInitializer>();

        return services;
    }

    private static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<IBorrowRecordService, BorrowRecordService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<RoleSeeder>();

        return services;
    }
}

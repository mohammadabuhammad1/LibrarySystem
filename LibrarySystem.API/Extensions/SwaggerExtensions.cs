using Microsoft.OpenApi.Models;

namespace LibrarySystem.API.Extensions;

public static class SwaggerExtensions
{
    private static readonly string[] BearerScopes = ["Bearer"];

    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Library Management API",
                Version = "v1",
                Description = "A comprehensive library management system API"
            });

            OpenApiSecurityScheme securitySchema = new()
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            c.AddSecurityDefinition("Bearer", securitySchema);

            OpenApiSecurityRequirement securityRequirement = new()
            {
                { securitySchema, BearerScopes }
            };

            c.AddSecurityRequirement(securityRequirement);
        });

        return services;
    }
}

using LibrarySystem.API.Services;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Application.Services;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Infrastructure;
using LibrarySystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using static System.Net.Mime.MediaTypeNames;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add HttpContextAccessor FIRST (required for TenantProvider)
builder.Services.AddHttpContextAccessor();

// Add CORS for multi-tenant frontends
builder.Services.AddCors(options =>
{
    options.AddPolicy("MultiTenantCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global rate limiter
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.IsAuthenticated == true
                ? $"user-{httpContext.User.Identity.Name}"
                : $"anon-{httpContext.Connection.RemoteIpAddress}",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    // Specific policy for authentication endpoints
    options.AddPolicy("AuthPolicy", httpContext =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5, // Lower limit for auth endpoints
                Window = TimeSpan.FromMinutes(1)
            });
    });

    // Policy for tenant-specific endpoints
    options.AddPolicy("PerTenantPolicy", httpContext =>
    {
        string? tenantCode = httpContext.Request.Headers["X-Tenant-Code"].FirstOrDefault()
                            ?? httpContext.Request.Query["tenantCode"].FirstOrDefault()
                            ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"tenant-{tenantCode}",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 200, // Higher limit for authenticated tenant requests
                Window = TimeSpan.FromMinutes(1)
            });
    });

    // Policy for API endpoints
    options.AddPolicy("ApiPolicy", httpContext =>
    {
        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: httpContext.User.Identity?.IsAuthenticated == true
                ? $"api-user-{httpContext.User.Identity.Name}"
                : $"api-anon-{httpContext.Connection.RemoteIpAddress}",
            factory: partition => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                TokensPerPeriod = 20,
                AutoReplenishment = true
            });
    });

    // Configure status code for rate limited requests
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token).ConfigureAwait(false);
    };
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DbContextHealthCheck<LibraryDbContext>>("Database");

// ✅ INFRASTRUCTURE LAYER (Includes ALL repositories and UnitOfWork)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<LibraryDbContext>()
.AddDefaultTokenProviders();

// Authentication
string? jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret is not configured");
}

static byte[] GetSigningKey(string secret) => Encoding.UTF8.GetBytes(secret);
byte[] signingKey = GetSigningKey(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(signingKey),
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library Management API", Version = "v1" });

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

    static string[] GetBearerScopes() => ["Bearer"];
    OpenApiSecurityRequirement securityRequirement = new()
    {
        { securitySchema, GetBearerScopes() }
    };

    c.AddSecurityRequirement(securityRequirement);
});

// ✅ TENANT & APPLICATION SERVICES (Register after Infrastructure)
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IBorrowRecordService, BorrowRecordService>();
builder.Services.AddScoped<IUserService, UserService>();

// ✅ ROLE SERVICES
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<RoleSeeder>();

builder.Services.AddAutoMapper(typeof(LibrarySystem.Application.Dtos.Books.BookDto).Assembly);
// ✅ DATA SEEDING
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<DatabaseInitializer>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Global error handling endpoint
app.Map("/error", (HttpContext context) =>
{
    return Results.Problem("An unexpected error occurred.");
});

app.UseHttpsRedirection();

// Use Rate Limiting (before authentication to protect auth endpoints)
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Add CORS
app.UseCors("MultiTenantCors");

// Add Health Checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                exception = entry.Value.Exception?.Message
            })
        });

        await context.Response.WriteAsync(result).ConfigureAwait(false);
    }
});

app.MapControllers();

// Run database initialization
await InitializeDatabase(app).ConfigureAwait(false);
await app.RunAsync().ConfigureAwait(false);

async Task InitializeDatabase(WebApplication app)
{
    using IServiceScope scope = app.Services.CreateScope();
    IServiceProvider services = scope.ServiceProvider;

    try
    {
        DatabaseInitializer dbInitializer = services.GetRequiredService<DatabaseInitializer>();
        await dbInitializer.InitializeAsync().ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Database initialization failed", ex);
    }
}
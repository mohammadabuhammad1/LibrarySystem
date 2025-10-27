namespace LibrarySystem.API.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Configure development vs production
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

        // HTTPS redirection
        app.UseHttpsRedirection();

        // Rate limiting (before authentication)
        app.UseRateLimiter();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Health checks
        app.MapComprehensiveHealthChecks();

        // Controllers
        app.MapControllers();

        return app;
    }
}

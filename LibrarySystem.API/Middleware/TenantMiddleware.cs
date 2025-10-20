using LibrarySystem.Application.Interfaces;
using Microsoft.Extensions.Primitives;

namespace LibrarySystem.API.Middleware;

public class TenantMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tenantProvider);

        // Try to resolve tenant from various sources
        await ResolveTenantAsync(context, tenantProvider).ConfigureAwait(false);

        await _next(context).ConfigureAwait(false);
    }

    private static async Task ResolveTenantAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        // Local function to simplify tenant resolution
        async Task<bool> TrySetTenantByCodeAsync(string? code)
        {
            return !string.IsNullOrEmpty(code) &&
                   await tenantProvider.SetCurrentTenantByCodeAsync(code!).ConfigureAwait(false);
        }

        // 1. Check URL path (e.g., /api/tenant/{code}/books)
        if (await TrySetTenantByCodeAsync(ExtractTenantCodeFromPath(context.Request.Path.ToString())).ConfigureAwait(false))
        {
            return;
        }

        // 2. Check header
        if (context.Request.Headers.TryGetValue("X-Tenant-Code", out StringValues tenantCodeFromHeader) &&
            await TrySetTenantByCodeAsync(tenantCodeFromHeader).ConfigureAwait(false))
        {
            return;
        }

        // 3. Check query string
        if (context.Request.Query.TryGetValue("tenantCode", out StringValues tenantCodeFromQuery) &&
            await TrySetTenantByCodeAsync(tenantCodeFromQuery).ConfigureAwait(false))
        {
            return;
        }

        // 4. For super admin, no tenant is set (they can access all)
        if (context.User.IsInRole("SuperAdmin"))
        {
            tenantProvider.ClearCurrentTenant();
        }
    }

    private static string? ExtractTenantCodeFromPath(string path)
    {
        // Example: /api/tenant/0001/books -> returns "0001"
        string[] segments = path.Split('/');
        for (int i = 0; i < segments.Length - 1; i++)
        {
            if (segments[i].Equals("tenant", StringComparison.OrdinalIgnoreCase) &&
                i + 1 < segments.Length)
            {
                return segments[i + 1];
            }
        }
        return null;
    }
}
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LibrarySystem.Infrastructure.Services;

public class TenantProvider(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork) : ITenantProvider
{
    public int? GetCurrentTenantId()
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.Items.TryGetValue("CurrentTenantId", out var tenantIdObj) == true)
        {
            return tenantIdObj as int?;
        }

        System.Security.Claims.Claim? tenantIdClaim = httpContext?.User.FindFirst("TenantId");
        if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tenantId))
        {
            return tenantId;
        }

        bool isSuperAdmin = httpContext?.User.IsInRole("SuperAdmin") == true;
        if (isSuperAdmin)
        {
            return null;
        }

        return null;
    }

    public string? GetCurrentTenantCode()
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.Items.TryGetValue("CurrentTenantCode", out var tenantCodeObj) == true)
        {
            return tenantCodeObj as string;
        }

        return httpContext?.User.FindFirst("TenantCode")?.Value;
    }

    public async Task<bool> SetCurrentTenantAsync(int tenantId)
    {
        OrganizationUnit? tenantOu = await unitOfWork.OrganizationUnits.GetByIdAsync(tenantId).ConfigureAwait(false);
        if (tenantOu?.Type != "Tenant" || !tenantOu.IsActive)
        {
            return false;
        }

        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Items["CurrentTenantId"] = tenantId;
            httpContext.Items["CurrentTenantCode"] = tenantOu.Code;
        }

        return true;
    }

    public async Task<bool> SetCurrentTenantByCodeAsync(string code)
    {
        OrganizationUnit? tenantOu = await unitOfWork.OrganizationUnits.GetByCodeAsync(code).ConfigureAwait(false);
        if (tenantOu?.Type != "Tenant" || !tenantOu.IsActive)
        {
            return false;
        }

        HttpContext httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Items["CurrentTenantId"] = tenantOu.Id;
            httpContext.Items["CurrentTenantCode"] = tenantOu.Code;
        }

        return true;
    }

    public void ClearCurrentTenant()
    {
        HttpContext httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Items.Remove("CurrentTenantId");
            httpContext.Items.Remove("CurrentTenantCode");
        }
    }
}
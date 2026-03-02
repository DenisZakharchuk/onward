using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Onward.Base.AspNetCore.Filters;

/// <summary>
/// Action filter that extracts the <c>tenant_id</c> claim from the current principal and
/// stores it in <see cref="HttpContext.Items"/> under the key <see cref="TenantIdKey"/>.
/// <para>
/// This makes the tenant identifier available to downstream services (e.g. data services
/// resolving <c>ITenantScopeFilter&lt;TEntity&gt;</c>) without requiring a generic
/// <c>ICurrentIdentityContext&lt;TOwnership&gt;</c> dependency.
/// </para>
/// <para>
/// Register via <c>services.AddOnwardTenantScoping()</c>. It is opt-in and should
/// only be used by bounded contexts that require multi-tenant data isolation.
/// </para>
/// </summary>
public sealed class TenantScopeActionFilter : IAsyncActionFilter
{
    /// <summary>Key used to store/retrieve the tenant ID in <see cref="HttpContext.Items"/>.</summary>
    public const string TenantIdKey = "Onward.TenantId";

    private const string TenantIdClaimType = "tenant_id";

    private readonly ILogger<TenantScopeActionFilter> _logger;

    public TenantScopeActionFilter(ILogger<TenantScopeActionFilter> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tenantIdClaim = context.HttpContext.User.FindFirstValue(TenantIdClaimType);

        if (!string.IsNullOrWhiteSpace(tenantIdClaim))
        {
            context.HttpContext.Items[TenantIdKey] = tenantIdClaim;
            _logger.LogDebug("Tenant context set to {TenantId} for request {Path}.",
                tenantIdClaim, context.HttpContext.Request.Path);
        }
        else if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            _logger.LogDebug("Authenticated request to {Path} carries no tenant_id claim — running without tenant scope.",
                context.HttpContext.Request.Path);
        }

        return next();
    }
}

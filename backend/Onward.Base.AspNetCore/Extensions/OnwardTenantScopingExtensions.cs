using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Onward.Base.AspNetCore.Filters;
using Onward.Base.DataAccess;

namespace Onward.Base.AspNetCore.Extensions;

/// <summary>
/// Service-collection extensions for Onward multi-tenant request scoping.
/// </summary>
public static class OnwardTenantScopingExtensions
{
    /// <summary>
    /// Registers the <see cref="TenantScopeActionFilter"/> as a global MVC action filter.
    /// <para>
    /// This is an opt-in extension intended for bounded contexts that require per-tenant
    /// data isolation. It extracts the <c>tenant_id</c> claim from the current principal
    /// and stores it in <c>HttpContext.Items</c> so that data services can apply
    /// <c>ITenantScopeFilter&lt;TEntity&gt;</c> without a generic ownership dependency.
    /// </para>
    /// <para>
    /// Call this after <c>AddControllers()</c>:
    /// <code>
    /// builder.Services.AddControllers();
    /// builder.Services.AddOnwardTenantScoping();
    /// </code>
    /// </para>
    /// </summary>
    public static IServiceCollection AddOnwardTenantScoping(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // MVC global filter — extracts tenant_id claim into HttpContext.Items on each request
        services.Configure<MvcOptions>(opts =>
            opts.Filters.AddService<TenantScopeActionFilter>());

        services.AddScoped<TenantScopeActionFilter>();

        // ITenantContext — resolved by DataServiceBase to apply ITenantScopeFilter<TEntity>
        services.AddScoped<ITenantContext, HttpContextTenantContext>();

        return services;
    }
}

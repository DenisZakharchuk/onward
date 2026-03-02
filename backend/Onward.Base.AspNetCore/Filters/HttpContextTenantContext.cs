using Microsoft.AspNetCore.Http;
using Onward.Base.DataAccess;

namespace Onward.Base.AspNetCore.Filters;

/// <summary>
/// ASP.NET Core implementation of <see cref="ITenantContext"/>.
/// Reads the tenant ID stored by <see cref="TenantScopeActionFilter"/> from
/// <c>HttpContext.Items</c> so data services can access it without resolving the
/// generic <c>ICurrentIdentityContext&lt;TOwnership&gt;</c>.
/// </summary>
public sealed class HttpContextTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string? CurrentTenantId =>
        _httpContextAccessor.HttpContext?.Items[TenantScopeActionFilter.TenantIdKey] as string;
}

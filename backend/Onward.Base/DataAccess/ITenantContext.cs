namespace Onward.Base.DataAccess;

/// <summary>
/// Provides the current request's tenant identifier, if any.
/// <para>
/// Implemented by <c>HttpContextTenantContext</c> in <c>Onward.Base.AspNetCore</c> when
/// <c>AddOnwardTenantScoping()</c> is called. Defaults to <c>null</c> when not registered.
/// </para>
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// The current tenant ID extracted from JWT claims, or <c>null</c> when no tenant
    /// context is present in the request (anonymous calls, single-tenant services, etc.).
    /// </summary>
    string? CurrentTenantId { get; }
}

/// <summary>
/// Null-object implementation returned when no tenant context is configured.
/// </summary>
internal sealed class NullTenantContext : ITenantContext
{
    public static readonly ITenantContext Instance = new NullTenantContext();
    public string? CurrentTenantId => null;
}

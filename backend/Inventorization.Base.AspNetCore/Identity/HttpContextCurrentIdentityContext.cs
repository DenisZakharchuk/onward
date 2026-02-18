using System.Security.Claims;
using Inventorization.Base.Abstractions;
using Inventorization.Base.Ownership;
using Microsoft.AspNetCore.Http;

namespace Inventorization.Base.AspNetCore.Identity;

/// <summary>
/// ASP.NET Core implementation of <see cref="ICurrentIdentityContext{TOwnership}"/>.
/// Reads identity claims from the current <see cref="HttpContext"/> and constructs
/// the ownership VO via the injected <see cref="IOwnershipFactory{TOwnership}"/>.
/// </summary>
/// <remarks>
/// <para>
/// The concrete <typeparamref name="TOwnership"/> VO shape is determined by the
/// <see cref="IOwnershipFactory{TOwnership}"/> registered in the DI container.
/// Typically this will be <see cref="Inventorization.Base.AspNetCore.Ownership.UserTenantOwnershipFactory"/>
/// for services that need both user and tenant identity.
/// </para>
/// <para>
/// Claim conventions:
/// <list type="bullet">
///   <item><c>ClaimTypes.NameIdentifier</c> → UserId (Guid)</item>
///   <item><c>"tenant_id"</c> → TenantId (Guid, optional)</item>
///   <item><c>ClaimTypes.Email</c> → Email</item>
///   <item><c>ClaimTypes.Role</c> → Roles (multiple claims supported)</item>
/// </list>
/// </para>
/// </remarks>
/// <typeparam name="TOwnership">Concrete ownership VO for this bounded context.</typeparam>
public sealed class HttpContextCurrentIdentityContext<TOwnership>
    : ICurrentIdentityContext<TOwnership>
    where TOwnership : OwnershipValueObject
{
    private const string TenantIdClaimType = "tenant_id";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOwnershipFactory<TOwnership> _ownershipFactory;

    // Lazily-computed, cached fields — safe because the HttpContext is request-scoped
    private TOwnership? _ownership;
    private bool _ownershipResolved;
    private IReadOnlyList<string>? _roles;

    public HttpContextCurrentIdentityContext(
        IHttpContextAccessor httpContextAccessor,
        IOwnershipFactory<TOwnership> ownershipFactory)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _ownershipFactory = ownershipFactory ?? throw new ArgumentNullException(nameof(ownershipFactory));
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    /// <inheritdoc />
    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated is true;

    /// <inheritdoc />
    public TOwnership? Ownership
    {
        get
        {
            if (_ownershipResolved)
                return _ownership;

            _ownershipResolved = true;

            if (!IsAuthenticated)
                return _ownership; // null

            var userIdClaim = Principal!.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return _ownership; // malformed claim — treat as anonymous

            var tenantIdClaim = Principal!.FindFirstValue(TenantIdClaimType);
            Guid.TryParse(tenantIdClaim, out var tenantId);  // tenantId stays Empty when missing

            _ownership = _ownershipFactory.Create(userId, tenantId == Guid.Empty ? null : tenantId);
            return _ownership;
        }
    }

    /// <inheritdoc />
    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    /// <inheritdoc />
    public IReadOnlyList<string> Roles =>
        _roles ??= Principal?
                       .FindAll(ClaimTypes.Role)
                       .Select(c => c.Value)
                       .ToList()
                       .AsReadOnly()
                   ?? (IReadOnlyList<string>)Array.Empty<string>();

    /// <inheritdoc />
    public bool IsInRole(string role) =>
        Principal?.IsInRole(role) is true;
}

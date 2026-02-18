using Inventorization.Base.Ownership;

namespace Inventorization.Base.AspNetCore.Ownership;

/// <summary>
/// Builds a <see cref="UserTenantOwnership"/> from the raw identity primitives
/// extracted from the HTTP authentication token.
/// Register as <c>IOwnershipFactory&lt;UserTenantOwnership&gt;</c> in API projects
/// that require both user and tenant/organisation partitioning.
/// When <paramref name="tenantId"/> is <c>null</c> this factory uses <see cref="Guid.Empty"/>
/// as a sentinel so the ownership VO is always non-null after authentication.
/// </summary>
public sealed class UserTenantOwnershipFactory : IOwnershipFactory<UserTenantOwnership>
{
    /// <inheritdoc />
    public UserTenantOwnership Create(Guid userId, Guid? tenantId)
        => new(userId, tenantId ?? Guid.Empty);
}

using Inventorization.Base.Ownership;

namespace Inventorization.Base.AspNetCore.Ownership;

/// <summary>
/// Builds a <see cref="UserOwnership"/> from the raw identity primitives
/// extracted from the HTTP authentication token.
/// Register as <c>IOwnershipFactory&lt;UserOwnership&gt;</c> in API projects
/// that use single-user ownership without tenancy.
/// </summary>
public sealed class UserOwnershipFactory : IOwnershipFactory<UserOwnership>
{
    /// <inheritdoc />
    public UserOwnership Create(Guid userId, Guid? tenantId) => new(userId);
}

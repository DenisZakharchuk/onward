using Inventorization.Base.Ownership;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Represents the identity of the current caller, expressed as an ownership value object.
/// Ownership identity fields (UserId, TenantId, etc.) are encapsulated inside
/// <typeparamref name="TOwnership"/> — they are never exposed as raw primitives on this interface.
/// </summary>
/// <remarks>
/// Implementations live in the presentation layer (e.g. Inventorization.Base.AspNetCore).
/// For background jobs and tests, register <see cref="AnonymousIdentityContext{TOwnership}"/>
/// as the default.
/// </remarks>
/// <typeparam name="TOwnership">
/// Concrete <see cref="OwnershipValueObject"/> derived type for this bounded context.
/// </typeparam>
public interface ICurrentIdentityContext<TOwnership>
    where TOwnership : OwnershipValueObject
{
    /// <summary>
    /// Ownership value object for the current caller.
    /// <c>null</c> when the caller is not authenticated.
    /// </summary>
    TOwnership? Ownership { get; }

    /// <summary>
    /// Email address claim from the authentication token.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// All role claims carried by the authentication token.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// <c>true</c> when the caller has a valid, authenticated identity.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks whether the caller holds the specified role.
    /// </summary>
    bool IsInRole(string role);
}

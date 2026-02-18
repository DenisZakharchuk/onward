namespace Inventorization.Base.Ownership;

/// <summary>
/// Constructs a concrete <see cref="OwnershipValueObject"/> from raw identity
/// primitives extracted from the authentication token.
/// Implementations live in the presentation layer (e.g. Inventorization.Base.AspNetCore)
/// so the domain layer never depends on HTTP concerns.
/// </summary>
/// <typeparam name="TOwnership">The concrete ownership VO produced by this factory.</typeparam>
public interface IOwnershipFactory<out TOwnership> where TOwnership : OwnershipValueObject
{
    /// <summary>
    /// Creates the ownership VO from the caller's identity.
    /// </summary>
    /// <param name="userId">Primary key of the authenticated user.</param>
    /// <param name="tenantId">Optional tenant/organisation identifier.</param>
    TOwnership Create(Guid userId, Guid? tenantId);
}

using Inventorization.Base.Ownership;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Domain-level service for authorisation decisions that require inspecting
/// entity ownership alongside the caller's identity.
/// Concrete implementations live in the presentation layer so they can read
/// role/permission claims without coupling domain code to HTTP.
/// </summary>
/// <typeparam name="TOwnership">
/// Concrete <see cref="OwnershipValueObject"/> for this bounded context.
/// </typeparam>
public interface ICurrentUserService<TOwnership>
    where TOwnership : OwnershipValueObject
{
    /// <summary>
    /// Returns <c>true</c> when the current caller holds a permission for the
    /// given resource and action.  Convention: <c>"{resource}.{action}"</c>
    /// maps to a role claim (e.g. <c>"product.create"</c>).
    /// </summary>
    Task<bool> HasPermissionAsync(string resource, string action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> when the current caller may access the entity:
    /// either they own it (their <typeparamref name="TOwnership"/> matches the
    /// entity's <see cref="IOwnedEntity{TOwnership}.Ownership"/>) or they hold
    /// a role that grants cross-ownership access (e.g. Admin).
    /// </summary>
    Task<bool> CanAccessEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : IOwnedEntity<TOwnership>;
}

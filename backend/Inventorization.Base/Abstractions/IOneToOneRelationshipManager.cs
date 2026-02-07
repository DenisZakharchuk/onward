using Inventorization.Base.Models;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Manages one-to-one relationships between entities
/// </summary>
/// <typeparam name="TEntity">Primary entity type</typeparam>
/// <typeparam name="TRelatedEntity">Related entity type</typeparam>
public interface IOneToOneRelationshipManager<TEntity, TRelatedEntity>
    where TEntity : class
    where TRelatedEntity : class
{
    /// <summary>
    /// Metadata describing the relationship type, cardinality, and entities involved
    /// </summary>
    RelationshipMetadata Metadata { get; }

    /// <summary>
    /// Gets the related entity ID for the specified entity
    /// </summary>
    /// <param name="entityId">ID of the primary entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Related entity ID, or null if no relationship exists</returns>
    Task<Guid?> GetRelatedIdAsync(Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets or updates the related entity for the primary entity
    /// </summary>
    /// <param name="entityId">ID of the primary entity</param>
    /// <param name="relatedEntityId">ID of the related entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if set/updated, false if already set to same value</returns>
    Task<bool> SetRelatedEntityAsync(Guid entityId, Guid relatedEntityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the relationship (sets foreign key to null if optional)
    /// </summary>
    /// <param name="entityId">ID of the primary entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if removed, false if no relationship existed</returns>
    Task<bool> RemoveRelationshipAsync(Guid entityId, CancellationToken cancellationToken = default);
}

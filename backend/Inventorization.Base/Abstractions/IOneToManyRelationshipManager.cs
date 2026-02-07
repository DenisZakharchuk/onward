using Inventorization.Base.Models;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Manages one-to-many relationships between a parent entity and its children
/// </summary>
/// <typeparam name="TParent">Parent entity type (the "one" side)</typeparam>
/// <typeparam name="TChild">Child entity type (the "many" side)</typeparam>
public interface IOneToManyRelationshipManager<TParent, TChild>
    where TParent : class
    where TChild : class
{
    /// <summary>
    /// Metadata describing the relationship type, cardinality, and entities involved
    /// </summary>
    RelationshipMetadata Metadata { get; }

    /// <summary>
    /// Gets all child entities for the specified parent
    /// </summary>
    /// <param name="parentId">ID of the parent entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of child entity IDs</returns>
    Task<List<Guid>> GetChildIdsAsync(Guid parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a child entity to the parent's collection
    /// </summary>
    /// <param name="parentId">ID of the parent entity</param>
    /// <param name="childId">ID of the child entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if added, false if already associated</returns>
    Task<bool> AddChildAsync(Guid parentId, Guid childId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a child entity from the parent's collection
    /// </summary>
    /// <param name="parentId">ID of the parent entity</param>
    /// <param name="childId">ID of the child entity to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if removed, false if not found</returns>
    Task<bool> RemoveChildAsync(Guid parentId, Guid childId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces all children for the parent entity
    /// </summary>
    /// <param name="parentId">ID of the parent entity</param>
    /// <param name="childIds">New set of child IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of children after replacement</returns>
    Task<int> ReplaceChildrenAsync(Guid parentId, List<Guid> childIds, CancellationToken cancellationToken = default);
}

using Microsoft.AspNetCore.Mvc;
using Inventorization.Base.DTOs;

namespace InventorySystem.API.Base.Controllers;

/// <summary>
/// Interface for controllers that manage entity relationships.
/// Controllers should implement this interface for each relationship they manage.
/// Use explicit interface implementation to support multiple relationships per controller.
/// </summary>
/// <typeparam name="TRelatedEntity">The type of related entity</typeparam>
public interface IRelationController<TRelatedEntity>
    where TRelatedEntity : class
{
    /// <summary>
    /// Updates relationships by adding and removing related entities for a single parent entity.
    /// HTTP PATCH /{controller}/{id}/relationships/{relationName}
    /// </summary>
    /// <param name="id">Parent entity ID</param>
    /// <param name="changes">Relationship changes (IDs to add/remove)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the relationship update operation</returns>
    Task<ActionResult<ServiceResult<RelationshipUpdateResult>>> UpdateRelationshipsAsync(
        Guid id,
        EntityReferencesDTO changes,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates relationships for multiple parent entities in a single bulk operation.
    /// HTTP PATCH /{controller}/relationships/{relationName}/bulk
    /// </summary>
    /// <param name="changes">Dictionary mapping parent entity IDs to their relationship changes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the bulk update operation</returns>
    Task<ActionResult<ServiceResult<BulkRelationshipUpdateResult>>> UpdateMultipleRelationshipsAsync(
        Dictionary<Guid, EntityReferencesDTO> changes,
        CancellationToken cancellationToken = default);
}

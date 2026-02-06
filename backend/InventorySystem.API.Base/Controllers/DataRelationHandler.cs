using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Inventorization.Base.DTOs;
using Inventorization.Base.Abstractions;

namespace InventorySystem.API.Base.Controllers;

/// <summary>
/// Abstract base class for handling relationship updates in controllers.
/// Provides boilerplate implementation for IRelationController methods.
/// </summary>
/// <typeparam name="TEntity">Parent entity type</typeparam>
/// <typeparam name="TRelatedEntity">Related entity type</typeparam>
public abstract class DataRelationHandler<TEntity, TRelatedEntity>
    where TEntity : class
    where TRelatedEntity : class
{
    protected readonly IRelationshipManager<TEntity, TRelatedEntity> RelationshipManager;
    protected readonly ILogger Logger;
    protected readonly string EntityName;
    protected readonly string RelatedEntityName;

    protected DataRelationHandler(
        IRelationshipManager<TEntity, TRelatedEntity> relationshipManager,
        ILogger logger)
    {
        RelationshipManager = relationshipManager ?? throw new ArgumentNullException(nameof(relationshipManager));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        EntityName = typeof(TEntity).Name;
        RelatedEntityName = typeof(TRelatedEntity).Name;
    }

    /// <summary>
    /// Handles single entity relationship update with logging and error handling
    /// </summary>
    /// <param name="id">Parent entity ID</param>
    /// <param name="changes">Relationship changes to apply</param>
    /// <param name="relationshipName">Display name for the relationship (e.g., "Roles", "Permissions")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected async Task<ActionResult<ServiceResult<RelationshipUpdateResult>>> HandleUpdateRelationshipsAsync(
        Guid id,
        EntityReferencesDTO changes,
        string relationshipName,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "Updating {RelationshipName} relationships for {EntityName} {EntityId}: +{AddCount} -{RemoveCount}",
            relationshipName,
            EntityName,
            id,
            changes?.IdsToAdd?.Count ?? 0,
            changes?.IdsToRemove?.Count ?? 0);

        try
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ServiceResult<RelationshipUpdateResult>.Failure("Invalid entity ID"));
            }

            if (changes == null || !changes.HasChanges)
            {
                return new BadRequestObjectResult(
                    ServiceResult<RelationshipUpdateResult>.Failure("No relationship changes specified"));
            }

            var result = await RelationshipManager.UpdateRelationshipsAsync(id, changes, cancellationToken);

            if (!result.IsSuccess)
            {
                Logger.LogWarning(
                    "Failed to update {RelationshipName} relationships for {EntityName} {EntityId}: {Message}",
                    relationshipName,
                    EntityName,
                    id,
                    result.Message);
                
                return new BadRequestObjectResult(ServiceResult<RelationshipUpdateResult>.Failure(
                    result.Message ?? "Failed to update relationships",
                    result.Errors));
            }

            Logger.LogInformation(
                "Successfully updated {RelationshipName} relationships for {EntityName} {EntityId}: +{Added} -{Removed}",
                relationshipName,
                EntityName,
                id,
                result.AddedCount,
                result.RemovedCount);

            return new OkObjectResult(ServiceResult<RelationshipUpdateResult>.Success(
                result,
                $"Updated {relationshipName} relationships: added {result.AddedCount}, removed {result.RemovedCount}"));
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Error updating {RelationshipName} relationships for {EntityName} {EntityId}",
                relationshipName,
                EntityName,
                id);
            
            return new ObjectResult(ServiceResult<RelationshipUpdateResult>.Failure(
                $"Internal server error: {ex.Message}"))
            {
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Handles bulk relationship updates with progress tracking and error handling
    /// </summary>
    /// <param name="changes">Dictionary mapping entity IDs to their relationship changes</param>
    /// <param name="relationshipName">Display name for the relationship (e.g., "Roles", "Permissions")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected async Task<ActionResult<ServiceResult<BulkRelationshipUpdateResult>>> HandleUpdateMultipleRelationshipsAsync(
        Dictionary<Guid, EntityReferencesDTO> changes,
        string relationshipName,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "Bulk updating {RelationshipName} relationships for {Count} {EntityName} entities",
            relationshipName,
            changes?.Count ?? 0,
            EntityName);

        try
        {
            if (changes == null || changes.Count == 0)
            {
                return new BadRequestObjectResult(
                    ServiceResult<BulkRelationshipUpdateResult>.Failure("No relationship changes specified"));
            }

            var result = await RelationshipManager.UpdateMultipleRelationshipsAsync(changes, cancellationToken);

            if (!result.IsSuccess)
            {
                Logger.LogWarning(
                    "Bulk update of {RelationshipName} relationships partially failed: {Successful} successful, {Failed} failed",
                    relationshipName,
                    result.SuccessfulOperations,
                    result.FailedOperations);
                
                return new BadRequestObjectResult(ServiceResult<BulkRelationshipUpdateResult>.Failure(
                    result.Message ?? "Bulk update failed",
                    result.Errors));
            }

            Logger.LogInformation(
                "Successfully bulk updated {RelationshipName} relationships: {Successful} entities, +{Added} -{Removed}",
                relationshipName,
                result.SuccessfulOperations,
                result.TotalAdded,
                result.TotalRemoved);

            return new OkObjectResult(ServiceResult<BulkRelationshipUpdateResult>.Success(
                result,
                $"Bulk update completed: {result.SuccessfulOperations} successful, added {result.TotalAdded}, removed {result.TotalRemoved}"));
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Error during bulk update of {RelationshipName} relationships",
                relationshipName);
            
            return new ObjectResult(ServiceResult<BulkRelationshipUpdateResult>.Failure(
                $"Internal server error: {ex.Message}"))
            {
                StatusCode = 500
            };
        }
    }
}

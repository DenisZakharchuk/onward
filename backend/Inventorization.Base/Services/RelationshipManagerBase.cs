using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;
using Inventorization.Base.DTOs;
using Inventorization.Base.Models;

namespace Inventorization.Base.Services;

/// <summary>
/// Generic base class for managing many-to-many relationships between entities.
/// Handles add/remove semantics via junction entities.
/// </summary>
/// <typeparam name="TEntity">Parent entity type</typeparam>
/// <typeparam name="TRelatedEntity">Related entity type</typeparam>
/// <typeparam name="TJunctionEntity">Junction entity type (e.g., UserRole, RolePermission)</typeparam>
public abstract class RelationshipManagerBase<TEntity, TRelatedEntity, TJunctionEntity>
    : IRelationshipManager<TEntity, TRelatedEntity>
    where TEntity : class
    where TRelatedEntity : class
    where TJunctionEntity : JunctionEntityBase
{
    protected readonly IRepository<TEntity> EntityRepository;
    protected readonly IRepository<TRelatedEntity> RelatedEntityRepository;
    protected readonly IRepository<TJunctionEntity> JunctionRepository;
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger Logger;

    protected readonly string EntityName;
    protected readonly string RelatedEntityName;

    /// <summary>
    /// Property accessor for extracting entity ID from junction entity.
    /// Resolved from IServiceProvider via IEntityIdPropertyAccessor{TJunctionEntity}.
    /// </summary>
    protected readonly IPropertyAccessor<TJunctionEntity, Guid> EntityIdAccessor;

    /// <summary>
    /// Property accessor for extracting related entity ID from junction entity.
    /// Resolved from IServiceProvider via IRelatedEntityIdPropertyAccessor{TJunctionEntity}.
    /// </summary>
    protected readonly IPropertyAccessor<TJunctionEntity, Guid> RelatedEntityIdAccessor;

    /// <summary>
    /// Metadata describing the relationship.
    /// Derived classes must provide metadata via constructor.
    /// </summary>
    public IRelationshipMetadata<TEntity, TRelatedEntity> Metadata { get; }

    protected RelationshipManagerBase(
        IRepository<TEntity> entityRepository,
        IRepository<TRelatedEntity> relatedEntityRepository,
        IRepository<TJunctionEntity> junctionRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger logger,
        IRelationshipMetadata<TEntity, TRelatedEntity> metadata)
    {
        EntityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
        RelatedEntityRepository = relatedEntityRepository ?? throw new ArgumentNullException(nameof(relatedEntityRepository));
        JunctionRepository = junctionRepository ?? throw new ArgumentNullException(nameof(junctionRepository));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));

        EntityName = typeof(TEntity).Name;
        RelatedEntityName = typeof(TRelatedEntity).Name;

        // Resolve property accessors from DI container
        EntityIdAccessor = serviceProvider.GetRequiredService<IEntityIdPropertyAccessor<TJunctionEntity>>();
        RelatedEntityIdAccessor = serviceProvider.GetRequiredService<IRelatedEntityIdPropertyAccessor<TJunctionEntity>>();
        
        // Validate metadata matches actual types
        if (Metadata.Type != RelationshipType.ManyToMany)
        {
            throw new InvalidOperationException(
                $"RelationshipManagerBase is designed for ManyToMany relationships only. " +
                $"Metadata indicates {Metadata.Type}.");
        }
    }

    /// <summary>
    /// Factory function to create junction entity instances.
    /// Override only if custom instantiation logic is needed.
    /// Default implementation uses reflection to call the two-parameter constructor.
    /// Example: (userId, roleId) => new UserRole(userId, roleId)
    /// </summary>
    protected virtual Func<Guid, Guid, TJunctionEntity> CreateJunctionEntity => ConstructJunctionEntity;

    public async Task<RelationshipUpdateResult> UpdateRelationshipsAsync(
        Guid entityId,
        EntityReferencesDTO changes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate parent entity exists
            var entityExists = await EntityRepository.ExistsAsync(entityId, cancellationToken);
            if (!entityExists)
                return RelationshipUpdateResult.Failure($"{EntityName} {entityId} not found");

            // Validate changes
            var validator = ServiceProvider.GetRequiredService<IValidator<EntityReferencesDTO>>();
            var validationResult = await validator.ValidateAsync(changes, cancellationToken);

            if (!validationResult.IsValid)
                return RelationshipUpdateResult.Failure("Validation failed", validationResult.Errors);

            int addedCount = 0;
            int removedCount = 0;

            // Remove relationships
            if (changes.IdsToRemove != null && changes.IdsToRemove.Any())
            {
                var entityIdPredicate = BuildEntityIdPredicate(entityId, changes.IdsToRemove);
                var existingRelationships = await JunctionRepository.FindAsync(
                    entityIdPredicate,
                    cancellationToken);

                foreach (var relationship in existingRelationships)
                {
                    await JunctionRepository.DeleteAsync(relationship.Id, cancellationToken);
                    removedCount++;
                }

                Logger.LogInformation(
                    "Removed {Count} {RelatedEntityName} relationships for {EntityName} {EntityId}",
                    removedCount,
                    RelatedEntityName,
                    EntityName,
                    entityId);
            }

            // Add relationships
            if (changes.IdsToAdd != null && changes.IdsToAdd.Any())
            {
                // Check for duplicate relationships
                var duplicateCheckPredicate = BuildEntityIdPredicate(entityId, changes.IdsToAdd);
                var existingRelationshipIds = (await JunctionRepository.FindAsync(
                    duplicateCheckPredicate,
                    cancellationToken))
                    .Select(CompileRelatedIdSelector())
                    .ToHashSet();

                foreach (var relatedId in changes.IdsToAdd)
                {
                    if (!existingRelationshipIds.Contains(relatedId))
                    {
                        var junctionEntity = CreateJunctionEntity(entityId, relatedId);
                        await JunctionRepository.CreateAsync(junctionEntity, cancellationToken);
                        addedCount++;
                    }
                    else
                    {
                        Logger.LogWarning(
                            "Skipped duplicate {RelatedEntityName} {RelatedId} for {EntityName} {EntityId}",
                            RelatedEntityName,
                            relatedId,
                            EntityName,
                            entityId);
                    }
                }

                Logger.LogInformation(
                    "Added {Count} {RelatedEntityName} relationships for {EntityName} {EntityId}",
                    addedCount,
                    RelatedEntityName,
                    EntityName,
                    entityId);
            }

            await UnitOfWork.SaveChangesAsync(cancellationToken);

            return RelationshipUpdateResult.Success(addedCount, removedCount);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Error updating {EntityName}-{RelatedEntityName} relationships for {EntityName} {EntityId}",
                EntityName,
                RelatedEntityName,
                EntityName,
                entityId);
            return RelationshipUpdateResult.Failure($"Failed to update relationships: {ex.Message}");
        }
    }

    public async Task<BulkRelationshipUpdateResult> UpdateMultipleRelationshipsAsync(
        Dictionary<Guid, EntityReferencesDTO> changes,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<Guid, RelationshipUpdateResult>();
        int totalAdded = 0;
        int totalRemoved = 0;
        int successful = 0;
        int failed = 0;
        var errors = new List<string>();

        Logger.LogInformation(
            "Starting bulk update of {EntityName}-{RelatedEntityName} relationships for {Count} entities",
            EntityName,
            RelatedEntityName,
            changes.Count);

        foreach (var (entityId, entityChanges) in changes)
        {
            try
            {
                var result = await UpdateRelationshipsAsync(entityId, entityChanges, cancellationToken);
                results[entityId] = result;

                if (result.IsSuccess)
                {
                    totalAdded += result.AddedCount;
                    totalRemoved += result.RemovedCount;
                    successful++;
                }
                else
                {
                    failed++;
                    errors.AddRange(result.Errors.Select(e => $"{EntityName} {entityId}: {e}"));
                }
            }
            catch (Exception ex)
            {
                failed++;
                var errorMsg = $"{EntityName} {entityId}: {ex.Message}";
                errors.Add(errorMsg);
                results[entityId] = RelationshipUpdateResult.Failure(ex.Message);

                Logger.LogError(ex, "Error in bulk update for {EntityName} {EntityId}", EntityName, entityId);
            }
        }

        Logger.LogInformation(
            "Bulk update completed: {Successful} successful, {Failed} failed, +{Added} -{Removed}",
            successful,
            failed,
            totalAdded,
            totalRemoved);

        if (failed == 0)
            return BulkRelationshipUpdateResult.Success(totalAdded, totalRemoved, successful, results);
        else
            return BulkRelationshipUpdateResult.PartialSuccess(totalAdded, totalRemoved, successful, failed, results, errors);
    }

    public async Task<List<Guid>> GetRelatedIdsAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        var entityIdEqualsParam = BuildEntityIdEqualsPredicate(entityId);
        var relationships = await JunctionRepository.FindAsync(
            entityIdEqualsParam,
            cancellationToken);

        return relationships.Select(CompileRelatedIdSelector()).ToList();
    }

    /// <summary>
    /// Default junction entity constructor using reflection.
    /// Assumes junction entity has a constructor accepting two Guid parameters.
    /// </summary>
    private TJunctionEntity ConstructJunctionEntity(Guid entityId, Guid relatedEntityId)
    {
        var constructor = typeof(TJunctionEntity).GetConstructor(
            new[] { typeof(Guid), typeof(Guid) });

        if (constructor == null)
        {
            throw new InvalidOperationException(
                $"Junction entity type {typeof(TJunctionEntity).Name} must have a public constructor " +
                $"accepting two Guid parameters (entityId, relatedEntityId)");
        }

        return (TJunctionEntity)constructor.Invoke(new object[] { entityId, relatedEntityId });
    }

    /// <summary>
    /// Builds a predicate to filter junction entities by parent entity ID and related entity IDs.
    /// Example: j => j.UserId == entityId && relatedIds.Contains(j.RoleId)
    /// </summary>
    private Expression<Func<TJunctionEntity, bool>> BuildEntityIdPredicate(Guid entityId, List<Guid> relatedIds)
    {
        var param = Expression.Parameter(typeof(TJunctionEntity), "j");
        
        // Extract property from EntityIdAccessor
        var entityIdProperty = (EntityIdAccessor.PropertyExpression.Body as MemberExpression)!;
        var entityIdAccess = Expression.MakeMemberAccess(param, entityIdProperty.Member);
        var entityIdEquals = Expression.Equal(entityIdAccess, Expression.Constant(entityId));
        
        // Extract property from RelatedEntityIdAccessor
        var relatedIdProperty = (RelatedEntityIdAccessor.PropertyExpression.Body as MemberExpression)!;
        var relatedIdAccess = Expression.MakeMemberAccess(param, relatedIdProperty.Member);
        
        // Build Contains expression
        var containsMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(Guid));
        var containsCall = Expression.Call(null, containsMethod, Expression.Constant(relatedIds), relatedIdAccess);
        
        // Combine with AND
        var combined = Expression.AndAlso(entityIdEquals, containsCall);
        
        return Expression.Lambda<Func<TJunctionEntity, bool>>(combined, param);
    }

    /// <summary>
    /// Builds a predicate to filter junction entities by parent entity ID only.
    /// Example: j => j.UserId == entityId
    /// </summary>
    private Expression<Func<TJunctionEntity, bool>> BuildEntityIdEqualsPredicate(Guid entityId)
    {
        var param = Expression.Parameter(typeof(TJunctionEntity), "j");
        
        var entityIdProperty = (EntityIdAccessor.PropertyExpression.Body as MemberExpression)!;
        var entityIdAccess = Expression.MakeMemberAccess(param, entityIdProperty.Member);
        var entityIdEquals = Expression.Equal(entityIdAccess, Expression.Constant(entityId));
        
        return Expression.Lambda<Func<TJunctionEntity, bool>>(entityIdEquals, param);
    }

    /// <summary>
    /// Compiles the RelatedEntityIdAccessor to a Func for use in Select statements
    /// </summary>
    private Func<TJunctionEntity, Guid> CompileRelatedIdSelector()
    {
        return RelatedEntityIdAccessor.CompiledGetter;
    }
}

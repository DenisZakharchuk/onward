using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;
using Inventorization.Base.Models;

namespace Inventorization.Base.Services;

/// <summary>
/// Generic base class for managing one-to-one relationships.
/// One entity has a foreign key pointing to the related entity.
/// </summary>
/// <typeparam name="TEntity">Primary entity type (the one with the foreign key)</typeparam>
/// <typeparam name="TRelatedEntity">Related entity type</typeparam>
public abstract class OneToOneRelationshipManagerBase<TEntity, TRelatedEntity>
    : IOneToOneRelationshipManager<TEntity, TRelatedEntity>
    where TEntity : class
    where TRelatedEntity : class
{
    protected readonly IRepository<TEntity> EntityRepository;
    protected readonly IRepository<TRelatedEntity> RelatedEntityRepository;
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly ILogger Logger;

    protected readonly string EntityName;
    protected readonly string RelatedEntityName;

    /// <summary>
    /// Property accessor for extracting related entity ID from primary entity.
    /// Example: For User -> UserProfile relationship, this would access UserProfileId property
    /// </summary>
    protected readonly IPropertyAccessor<TEntity, Guid?> RelatedIdAccessor;

    /// <summary>
    /// Metadata describing the relationship
    /// </summary>
    public IRelationshipMetadata<TEntity, TRelatedEntity> Metadata { get; }

    protected OneToOneRelationshipManagerBase(
        IRepository<TEntity> entityRepository,
        IRepository<TRelatedEntity> relatedEntityRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger logger,
        IRelationshipMetadata<TEntity, TRelatedEntity> metadata,
        Type relatedIdAccessorType)
    {
        EntityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
        RelatedEntityRepository = relatedEntityRepository ?? throw new ArgumentNullException(nameof(relatedEntityRepository));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));

        EntityName = typeof(TEntity).Name;
        RelatedEntityName = typeof(TRelatedEntity).Name;

        // Resolve related ID accessor from DI
        RelatedIdAccessor = (IPropertyAccessor<TEntity, Guid?>)serviceProvider.GetRequiredService(relatedIdAccessorType);

        // Validate metadata
        if (Metadata.Type != RelationshipType.OneToOne)
        {
            throw new InvalidOperationException(
                $"OneToOneRelationshipManagerBase is designed for OneToOne relationships only. " +
                $"Metadata indicates {Metadata.Type}.");
        }
    }

    /// <summary>
    /// Method to set the related entity ID on the primary entity.
    /// Must be implemented by derived classes since property setters are entity-specific.
    /// </summary>
    protected abstract void SetRelatedId(TEntity entity, Guid? relatedId);

    public async Task<Guid?> GetRelatedIdAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        var entity = await EntityRepository.GetByIdAsync(entityId, cancellationToken);
        if (entity == null)
        {
            Logger.LogWarning("{EntityName} {EntityId} not found", EntityName, entityId);
            return null;
        }

        return RelatedIdAccessor.GetValue(entity);
    }

    public async Task<bool> SetRelatedEntityAsync(Guid entityId, Guid relatedEntityId, CancellationToken cancellationToken = default)
    {
        // Verify entity exists
        var entity = await EntityRepository.GetByIdAsync(entityId, cancellationToken);
        if (entity == null)
        {
            Logger.LogWarning("{EntityName} {EntityId} not found", EntityName, entityId);
            return false;
        }

        // Verify related entity exists
        var relatedEntityExists = await RelatedEntityRepository.ExistsAsync(relatedEntityId, cancellationToken);
        if (!relatedEntityExists)
        {
            Logger.LogWarning("{RelatedEntityName} {RelatedEntityId} not found", RelatedEntityName, relatedEntityId);
            return false;
        }

        // Check if already set to same value
        var currentRelatedId = RelatedIdAccessor.GetValue(entity);
        if (currentRelatedId == relatedEntityId)
        {
            Logger.LogInformation("{EntityName} {EntityId} already associated with {RelatedEntityName} {RelatedEntityId}", 
                EntityName, entityId, RelatedEntityName, relatedEntityId);
            return false;
        }

        // Set related ID
        SetRelatedId(entity, relatedEntityId);
        await EntityRepository.UpdateAsync(entity, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Associated {EntityName} {EntityId} with {RelatedEntityName} {RelatedEntityId}", 
            EntityName, entityId, RelatedEntityName, relatedEntityId);
        
        return true;
    }

    public async Task<bool> RemoveRelationshipAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        var entity = await EntityRepository.GetByIdAsync(entityId, cancellationToken);
        if (entity == null)
        {
            Logger.LogWarning("{EntityName} {EntityId} not found", EntityName, entityId);
            return false;
        }

        var currentRelatedId = RelatedIdAccessor.GetValue(entity);
        if (currentRelatedId == null)
        {
            Logger.LogInformation("{EntityName} {EntityId} has no associated {RelatedEntityName}", 
                EntityName, entityId, RelatedEntityName);
            return false;
        }

        // Check cardinality
        if (Metadata.Cardinality == RelationshipCardinality.Required)
        {
            throw new InvalidOperationException(
                $"Cannot remove {RelatedEntityName} from {EntityName} {entityId}: relationship is required");
        }

        // Set related ID to null
        SetRelatedId(entity, null);
        await EntityRepository.UpdateAsync(entity, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Removed {RelatedEntityName} from {EntityName} {EntityId}", 
            RelatedEntityName, EntityName, entityId);
        
        return true;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;
using Inventorization.Base.Models;

namespace Inventorization.Base.Services;

/// <summary>
/// Generic base class for managing one-to-many relationships.
/// Child entities have a foreign key pointing to the parent.
/// </summary>
/// <typeparam name="TParent">Parent entity type (the "one" side)</typeparam>
/// <typeparam name="TChild">Child entity type (the "many" side)</typeparam>
public abstract class OneToManyRelationshipManagerBase<TParent, TChild>
    : IOneToManyRelationshipManager<TParent, TChild>
    where TParent : class
    where TChild : class
{
    protected readonly IRepository<TParent> ParentRepository;
    protected readonly IRepository<TChild> ChildRepository;
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly ILogger Logger;

    protected readonly string ParentName;
    protected readonly string ChildName;

    /// <summary>
    /// Property accessor for extracting parent ID from child entity.
    /// Resolved from IServiceProvider via IPropertyAccessor.
    /// Example: For RefreshToken -> User relationship, this would access UserId property
    /// </summary>
    protected readonly IPropertyAccessor<TChild, Guid> ParentIdAccessor;

    /// <summary>
    /// Metadata describing the relationship
    /// </summary>
    public IRelationshipMetadata<TParent, TChild> Metadata { get; }

    protected OneToManyRelationshipManagerBase(
        IRepository<TParent> parentRepository,
        IRepository<TChild> childRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger logger,
        IRelationshipMetadata<TParent, TChild> metadata,
        Type parentIdAccessorType)
    {
        ParentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
        ChildRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));

        ParentName = typeof(TParent).Name;
        ChildName = typeof(TChild).Name;

        // Resolve parent ID accessor from DI
        ParentIdAccessor = (IPropertyAccessor<TChild, Guid>)serviceProvider.GetRequiredService(parentIdAccessorType);

        // Validate metadata
        if (Metadata.Type != RelationshipType.OneToMany)
        {
            throw new InvalidOperationException(
                $"OneToManyRelationshipManagerBase is designed for OneToMany relationships only. " +
                $"Metadata indicates {Metadata.Type}.");
        }
    }

    /// <summary>
    /// Method to set the parent ID on a child entity.
    /// Must be implemented by derived classes since property setters are entity-specific.
    /// </summary>
    protected abstract void SetParentId(TChild child, Guid? parentId);

    public async Task<List<Guid>> GetChildIdsAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var predicate = BuildParentIdEqualsPredicate(parentId);
        var children = await ChildRepository.FindAsync(predicate, cancellationToken);
        
        return children.Select(c => GetEntityId(c)).ToList();
    }

    public async Task<bool> AddChildAsync(Guid parentId, Guid childId, CancellationToken cancellationToken = default)
    {
        // Verify parent exists
        var parentExists = await ParentRepository.ExistsAsync(parentId, cancellationToken);
        if (!parentExists)
        {
            Logger.LogWarning("{ParentName} {ParentId} not found", ParentName, parentId);
            return false;
        }

        // Get child entity
        var child = await ChildRepository.GetByIdAsync(childId, cancellationToken);
        if (child == null)
        {
            Logger.LogWarning("{ChildName} {ChildId} not found", ChildName, childId);
            return false;
        }

        // Check if already associated
        var currentParentId = ParentIdAccessor.GetValue(child);
        if (currentParentId == parentId)
        {
            Logger.LogInformation("{ChildName} {ChildId} already associated with {ParentName} {ParentId}", 
                ChildName, childId, ParentName, parentId);
            return false;
        }

        // Set parent ID
        SetParentId(child, parentId);
        await ChildRepository.UpdateAsync(child, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Associated {ChildName} {ChildId} with {ParentName} {ParentId}", 
            ChildName, childId, ParentName, parentId);
        
        return true;
    }

    public async Task<bool> RemoveChildAsync(Guid parentId, Guid childId, CancellationToken cancellationToken = default)
    {
        var child = await ChildRepository.GetByIdAsync(childId, cancellationToken);
        if (child == null)
        {
            Logger.LogWarning("{ChildName} {ChildId} not found", ChildName, childId);
            return false;
        }

        var currentParentId = ParentIdAccessor.GetValue(child);
        if (currentParentId != parentId)
        {
            Logger.LogWarning("{ChildName} {ChildId} is not associated with {ParentName} {ParentId}", 
                ChildName, childId, ParentName, parentId);
            return false;
        }

        // Set parent ID to null (if optional) or throw if required
        if (Metadata.Cardinality == RelationshipCardinality.Required)
        {
            throw new InvalidOperationException(
                $"Cannot remove {ChildName} {childId} from {ParentName} {parentId}: relationship is required");
        }

        SetParentId(child, null);
        await ChildRepository.UpdateAsync(child, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Removed {ChildName} {ChildId} from {ParentName} {ParentId}", 
            ChildName, childId, ParentName, parentId);
        
        return true;
    }

    public async Task<int> ReplaceChildrenAsync(Guid parentId, List<Guid> childIds, CancellationToken cancellationToken = default)
    {
        // Verify parent exists
        var parentExists = await ParentRepository.ExistsAsync(parentId, cancellationToken);
        if (!parentExists)
        {
            throw new InvalidOperationException($"{ParentName} {parentId} not found");
        }

        // Get current children
        var predicate = BuildParentIdEqualsPredicate(parentId);
        var currentChildren = await ChildRepository.FindAsync(predicate, cancellationToken);
        var currentChildIds = currentChildren.Select(c => GetEntityId(c)).ToHashSet();

        // Remove old children
        var toRemove = currentChildren.Where(c => !childIds.Contains(GetEntityId(c))).ToList();
        foreach (var child in toRemove)
        {
            if (Metadata.Cardinality == RelationshipCardinality.Required)
            {
                throw new InvalidOperationException(
                    $"Cannot remove {ChildName} {GetEntityId(child)} from {ParentName} {parentId}: relationship is required");
            }
            
            SetParentId(child, null);
            await ChildRepository.UpdateAsync(child, cancellationToken);
        }

        // Add new children
        var toAdd = childIds.Where(id => !currentChildIds.Contains(id)).ToList();
        foreach (var childId in toAdd)
        {
            var child = await ChildRepository.GetByIdAsync(childId, cancellationToken);
            if (child == null)
            {
                throw new InvalidOperationException($"{ChildName} {childId} not found");
            }

            SetParentId(child, parentId);
            await ChildRepository.UpdateAsync(child, cancellationToken);
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Replaced children for {ParentName} {ParentId}: removed {RemovedCount}, added {AddedCount}", 
            ParentName, parentId, toRemove.Count, toAdd.Count);

        return childIds.Count;
    }

    /// <summary>
    /// Builds a predicate to filter children by parent ID
    /// </summary>
    private System.Linq.Expressions.Expression<Func<TChild, bool>> BuildParentIdEqualsPredicate(Guid parentId)
    {
        var param = System.Linq.Expressions.Expression.Parameter(typeof(TChild), "c");
        var property = (ParentIdAccessor.PropertyExpression.Body as System.Linq.Expressions.MemberExpression)!;
        var propertyAccess = System.Linq.Expressions.Expression.MakeMemberAccess(param, property.Member);
        var equals = System.Linq.Expressions.Expression.Equal(propertyAccess, System.Linq.Expressions.Expression.Constant(parentId));
        
        return System.Linq.Expressions.Expression.Lambda<Func<TChild, bool>>(equals, param);
    }

    /// <summary>
    /// Gets the entity ID from a child entity.
    /// Assumes child has an Id property (from BaseEntity).
    /// </summary>
    private Guid GetEntityId(TChild child)
    {
        var idProperty = typeof(TChild).GetProperty("Id") 
            ?? throw new InvalidOperationException($"{ChildName} must have an Id property");
        
        return (Guid)idProperty.GetValue(child)!;
    }
}

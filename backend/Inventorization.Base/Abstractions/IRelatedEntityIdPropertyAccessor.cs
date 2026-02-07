namespace Inventorization.Base.Abstractions;

/// <summary>
/// Marker interface for property accessors that extract related entity IDs from junction entities.
/// Used by RelationshipManagerBase to resolve the correct accessor from DI container.
/// </summary>
/// <typeparam name="TJunctionEntity">The junction entity type</typeparam>
public interface IRelatedEntityIdPropertyAccessor<TJunctionEntity> : IPropertyAccessor<TJunctionEntity, Guid>
    where TJunctionEntity : class
{
}

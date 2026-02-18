using Inventorization.Base.Models;

namespace Inventorization.Base.Ownership;

/// <summary>
/// Combines <see cref="ISoftDeletableEntity"/> and <see cref="IOwnedEntity{TOwnership}"/>
/// for entities that require both ownership tracking and soft-deletion.
/// </summary>
/// <typeparam name="TOwnership">Concrete ownership value object for this entity.</typeparam>
public interface ISoftDeletableOwnedEntity<TOwnership>
    : ISoftDeletableEntity, IOwnedEntity<TOwnership>
    where TOwnership : OwnershipValueObject;

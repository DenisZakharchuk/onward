using Inventorization.Base.Models;

namespace Inventorization.Base.Ownership;

/// <summary>
/// Base entity class that adds ownership semantics to any entity with a generic primary key.
/// Stores the <typeparamref name="TOwnership"/> VO at creation and mutation time.
/// No EF navigation property is declared — the ownership fields are raw value columns,
/// keeping this bounded context decoupled from the Identity bounded context.
/// </summary>
/// <typeparam name="TOwnership">Concrete ownership value object for this entity.</typeparam>
/// <typeparam name="TPrimaryKey">Primary key type (int, Guid, etc.).</typeparam>
public abstract class OwnedBaseEntity<TOwnership, TPrimaryKey>
    : BaseEntity<TPrimaryKey>, IOwnedEntity<TOwnership>
    where TOwnership : OwnershipValueObject
    where TPrimaryKey : struct
{
    /// <summary>
    /// Ownership snapshot stamped at creation. EF Core maps this as an owned/complex type.
    /// </summary>
    public TOwnership? Ownership { get; protected set; }

    /// <summary>
    /// Ownership of the last modifier. Null until the entity is first updated.
    /// </summary>
    public TOwnership? LastModifiedOwnership { get; protected set; }

    /// <inheritdoc />
    public void SetOwnership(TOwnership ownership)
    {
        ArgumentNullException.ThrowIfNull(ownership);
        Ownership = ownership;
    }

    /// <inheritdoc />
    public void UpdateOwnership(TOwnership ownership)
    {
        ArgumentNullException.ThrowIfNull(ownership);
        LastModifiedOwnership = ownership;
    }
}

/// <summary>
/// Guid-primary-key shortcut. Mirrors the <c>BaseEntity : BaseEntity&lt;Guid&gt;</c> pattern.
/// Use this as the base class for all owned entities that use a <see cref="Guid"/> PK (the common case).
/// </summary>
/// <typeparam name="TOwnership">Concrete ownership value object for this entity.</typeparam>
public abstract class OwnedBaseEntity<TOwnership>
    : OwnedBaseEntity<TOwnership, Guid>
    where TOwnership : OwnershipValueObject;

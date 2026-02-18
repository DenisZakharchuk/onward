namespace Inventorization.Base.Ownership;

/// <summary>
/// Marks an entity as having ownership semantics.
/// Ownership is expressed as a value object so each bounded context
/// can carry exactly the identity fields it needs without coupling to
/// the Identity bounded context directly.
/// </summary>
/// <typeparam name="TOwnership">
/// Concrete <see cref="OwnershipValueObject"/> that encapsulates the
/// identity fields relevant to this entity (e.g. <see cref="UserOwnership"/>,
/// <see cref="UserTenantOwnership"/>).
/// </typeparam>
public interface IOwnedEntity<TOwnership> where TOwnership : OwnershipValueObject
{
    /// <summary>
    /// Ownership snapshot at creation time. Null only on un-persisted instances.
    /// </summary>
    TOwnership? Ownership { get; }

    /// <summary>
    /// Ownership snapshot of the last modifier. Null when the entity has never been updated.
    /// </summary>
    TOwnership? LastModifiedOwnership { get; }

    /// <summary>
    /// Stamps ownership at creation. Called once by <c>DataServiceBase</c> during <c>AddAsync</c>.
    /// </summary>
    void SetOwnership(TOwnership ownership);

    /// <summary>
    /// Updates the last-modified ownership. Called by <c>DataServiceBase</c> during <c>UpdateAsync</c>.
    /// </summary>
    void UpdateOwnership(TOwnership ownership);
}

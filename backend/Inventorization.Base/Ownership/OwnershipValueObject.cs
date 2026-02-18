namespace Inventorization.Base.Ownership;

/// <summary>
/// Abstract base record for all ownership value objects.
/// Bounded contexts derive concrete records carrying the identity fields
/// relevant to their access rules (e.g. UserId, TenantId).
/// </summary>
public abstract record OwnershipValueObject;

namespace Inventorization.Base.Ownership;

/// <summary>
/// Ownership value object carrying a single user identity.
/// Use when tenancy is not required — a resource belongs to one user.
/// </summary>
/// <param name="UserId">Primary key of the owning user in the Identity bounded context.</param>
public record UserOwnership(Guid UserId) : OwnershipValueObject;

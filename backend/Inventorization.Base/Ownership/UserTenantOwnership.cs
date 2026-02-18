namespace Inventorization.Base.Ownership;

/// <summary>
/// Ownership value object carrying both a user identity and a tenant (organisation) identity.
/// Use when resources are partitioned by organisation and by user within that organisation.
/// </summary>
/// <param name="UserId">Primary key of the owning user in the Identity bounded context.</param>
/// <param name="TenantId">Primary key of the owning tenant/organisation.</param>
public record UserTenantOwnership(Guid UserId, Guid TenantId) : OwnershipValueObject;

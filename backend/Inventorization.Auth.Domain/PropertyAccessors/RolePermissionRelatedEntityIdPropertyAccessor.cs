using Inventorization.Base.Abstractions;
using Inventorization.Auth.Domain.Entities;

namespace Inventorization.Auth.Domain.PropertyAccessors;

/// <summary>
/// Property accessor for extracting PermissionId (RelatedEntityId) from RolePermission junction entity
/// </summary>
public class RolePermissionRelatedEntityIdPropertyAccessor : PropertyAccessor<RolePermission, Guid>, IRelatedEntityIdPropertyAccessor<RolePermission>
{
    public RolePermissionRelatedEntityIdPropertyAccessor() : base(rp => rp.PermissionId)
    {
    }
}

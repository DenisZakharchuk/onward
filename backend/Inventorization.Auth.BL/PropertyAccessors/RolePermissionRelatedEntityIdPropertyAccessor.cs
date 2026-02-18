using Inventorization.Base.Abstractions;
using Inventorization.Auth.BL.Entities;

namespace Inventorization.Auth.BL.PropertyAccessors;

/// <summary>
/// Property accessor for extracting PermissionId (RelatedEntityId) from RolePermission junction entity
/// </summary>
public class RolePermissionRelatedEntityIdPropertyAccessor : PropertyAccessor<RolePermission, Guid>, IRelatedEntityIdPropertyAccessor<RolePermission>
{
    public RolePermissionRelatedEntityIdPropertyAccessor() : base(rp => rp.PermissionId)
    {
    }
}

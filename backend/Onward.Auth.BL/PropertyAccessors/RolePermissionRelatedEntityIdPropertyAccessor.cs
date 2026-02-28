using Onward.Base.Abstractions;
using Onward.Auth.BL.Entities;

namespace Onward.Auth.BL.PropertyAccessors;

/// <summary>
/// Property accessor for extracting PermissionId (RelatedEntityId) from RolePermission junction entity
/// </summary>
public class RolePermissionRelatedEntityIdPropertyAccessor : PropertyAccessor<RolePermission, Guid>, IRelatedEntityIdPropertyAccessor<RolePermission>
{
    public RolePermissionRelatedEntityIdPropertyAccessor() : base(rp => rp.PermissionId)
    {
    }
}

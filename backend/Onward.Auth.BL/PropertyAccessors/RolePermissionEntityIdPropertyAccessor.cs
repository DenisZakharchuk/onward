using Onward.Base.Abstractions;
using Onward.Auth.BL.Entities;

namespace Onward.Auth.BL.PropertyAccessors;

/// <summary>
/// Property accessor for extracting RoleId (EntityId) from RolePermission junction entity
/// </summary>
public class RolePermissionEntityIdPropertyAccessor : PropertyAccessor<RolePermission, Guid>, IEntityIdPropertyAccessor<RolePermission>
{
    public RolePermissionEntityIdPropertyAccessor() : base(rp => rp.RoleId)
    {
    }
}

using Inventorization.Base.Abstractions;
using Inventorization.Auth.BL.Entities;

namespace Inventorization.Auth.BL.PropertyAccessors;

/// <summary>
/// Property accessor for extracting RoleId (EntityId) from RolePermission junction entity
/// </summary>
public class RolePermissionEntityIdPropertyAccessor : PropertyAccessor<RolePermission, Guid>, IEntityIdPropertyAccessor<RolePermission>
{
    public RolePermissionEntityIdPropertyAccessor() : base(rp => rp.RoleId)
    {
    }
}

using Inventorization.Base.Abstractions;
using Inventorization.Auth.Domain.Entities;

namespace Inventorization.Auth.Domain.PropertyAccessors;

/// <summary>
/// Property accessor for extracting RoleId (EntityId) from RolePermission junction entity
/// </summary>
public class RolePermissionEntityIdPropertyAccessor : PropertyAccessor<RolePermission, Guid>, IEntityIdPropertyAccessor<RolePermission>
{
    public RolePermissionEntityIdPropertyAccessor() : base(rp => rp.RoleId)
    {
    }
}

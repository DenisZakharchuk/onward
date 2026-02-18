using Inventorization.Base.Abstractions;
using Inventorization.Auth.BL.Entities;

namespace Inventorization.Auth.BL.PropertyAccessors;

/// <summary>
/// Property accessor for extracting RoleId (RelatedEntityId) from UserRole junction entity
/// </summary>
public class UserRoleRelatedEntityIdPropertyAccessor : PropertyAccessor<UserRole, Guid>, IRelatedEntityIdPropertyAccessor<UserRole>
{
    public UserRoleRelatedEntityIdPropertyAccessor() : base(ur => ur.RoleId)
    {
    }
}

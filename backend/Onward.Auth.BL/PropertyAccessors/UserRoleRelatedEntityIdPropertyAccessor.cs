using Onward.Base.Abstractions;
using Onward.Auth.BL.Entities;

namespace Onward.Auth.BL.PropertyAccessors;

/// <summary>
/// Property accessor for extracting RoleId (RelatedEntityId) from UserRole junction entity
/// </summary>
public class UserRoleRelatedEntityIdPropertyAccessor : PropertyAccessor<UserRole, Guid>, IRelatedEntityIdPropertyAccessor<UserRole>
{
    public UserRoleRelatedEntityIdPropertyAccessor() : base(ur => ur.RoleId)
    {
    }
}

using Inventorization.Base.Abstractions;
using Inventorization.Auth.BL.Entities;

namespace Inventorization.Auth.BL.PropertyAccessors;

/// <summary>
/// Property accessor for extracting UserId (EntityId) from UserRole junction entity
/// </summary>
public class UserRoleEntityIdPropertyAccessor : PropertyAccessor<UserRole, Guid>, IEntityIdPropertyAccessor<UserRole>
{
    public UserRoleEntityIdPropertyAccessor() : base(ur => ur.UserId)
    {
    }
}

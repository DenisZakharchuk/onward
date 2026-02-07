using Inventorization.Base.Abstractions;
using Inventorization.Auth.Domain.Entities;

namespace Inventorization.Auth.Domain.PropertyAccessors;

/// <summary>
/// Property accessor for extracting UserId (EntityId) from UserRole junction entity
/// </summary>
public class UserRoleEntityIdPropertyAccessor : PropertyAccessor<UserRole, Guid>, IEntityIdPropertyAccessor<UserRole>
{
    public UserRoleEntityIdPropertyAccessor() : base(ur => ur.UserId)
    {
    }
}

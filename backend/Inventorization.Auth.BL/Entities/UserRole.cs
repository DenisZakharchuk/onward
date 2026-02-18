using Inventorization.Base.Models;

namespace Inventorization.Auth.BL.Entities;

/// <summary>
/// Junction table for User-Role relationships (many-to-many)
/// </summary>
public class UserRole : JunctionEntityBase
{
    /// <summary>
    /// Creates a new user-role assignment
    /// </summary>
    public UserRole(Guid userId, Guid roleId) : base(userId, roleId)
    {
    }

    /// <summary>
    /// Foreign key to User. Aliases EntityId from base class.
    /// </summary>
    public Guid UserId => EntityId;

    /// <summary>
    /// Foreign key to Role. Aliases RelatedEntityId from base class.
    /// </summary>
    public Guid RoleId => RelatedEntityId;

    // Navigation properties
    public User User { get; } = null!;
    public Role Role { get; } = null!;
}

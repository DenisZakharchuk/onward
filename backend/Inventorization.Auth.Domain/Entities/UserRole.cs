using Inventorization.Base.Models;

namespace Inventorization.Auth.Domain.Entities;

/// <summary>
/// Junction table for User-Role relationships (many-to-many)
/// </summary>
public class UserRole : BaseEntity
{
    private UserRole() { }  // EF Core only

    /// <summary>
    /// Creates a new user-role assignment
    /// </summary>
    public UserRole(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID is required", nameof(userId));
        if (roleId == Guid.Empty) throw new ArgumentException("Role ID is required", nameof(roleId));
        
        UserId = userId;
        RoleId = roleId;
    }

    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    // Navigation properties
    public User User { get; } = null!;
    public Role Role { get; } = null!;
}

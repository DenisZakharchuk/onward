using Inventorization.Base.Models;

namespace Inventorization.Auth.BL.Entities;

/// <summary>
/// Junction table for Role-Permission relationships (many-to-many)
/// </summary>
public class RolePermission : JunctionEntityBase
{
    /// <summary>
    /// Creates a new role-permission assignment
    /// </summary>
    public RolePermission(Guid roleId, Guid permissionId) : base(roleId, permissionId)
    {
    }

    /// <summary>
    /// Foreign key to Role. Aliases EntityId from base class.
    /// </summary>
    public Guid RoleId => EntityId;

    /// <summary>
    /// Foreign key to Permission. Aliases RelatedEntityId from base class.
    /// </summary>
    public Guid PermissionId => RelatedEntityId;

    // Navigation properties
    public Role Role { get; } = null!;
    public Permission Permission { get; } = null!;
}

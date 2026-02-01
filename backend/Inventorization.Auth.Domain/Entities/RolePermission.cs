using Inventorization.Base.Models;

namespace Inventorization.Auth.Domain.Entities;

/// <summary>
/// Junction table for Role-Permission relationships (many-to-many)
/// </summary>
public class RolePermission : BaseEntity
{
    private RolePermission() { }  // EF Core only

    /// <summary>
    /// Creates a new role-permission assignment
    /// </summary>
    public RolePermission(Guid roleId, Guid permissionId)
    {
        if (roleId == Guid.Empty) throw new ArgumentException("Role ID is required", nameof(roleId));
        if (permissionId == Guid.Empty) throw new ArgumentException("Permission ID is required", nameof(permissionId));
        
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    // Navigation properties
    public Role Role { get; } = null!;
    public Permission Permission { get; } = null!;
}

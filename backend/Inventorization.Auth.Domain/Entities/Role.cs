using Inventorization.Base.Models;

namespace Inventorization.Auth.Domain.Entities;

/// <summary>
/// Represents a role that can be assigned to users
/// </summary>
public class Role : BaseEntity
{
    private Role() { }  // EF Core only

    /// <summary>
    /// Creates a new role with the provided name and optional description
    /// </summary>
    public Role(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required", nameof(name));
        
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; } = null!;  // Admin, Manager, Viewer, User
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; } = new List<RolePermission>();

    /// <summary>
    /// Updates role information
    /// </summary>
    public void Update(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required", nameof(name));
        
        Name = name;
        Description = description;
    }
}

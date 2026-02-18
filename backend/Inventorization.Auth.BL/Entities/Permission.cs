using Inventorization.Base.Models;

namespace Inventorization.Auth.BL.Entities;

/// <summary>
/// Represents a permission that can be assigned to roles
/// </summary>
public class Permission : BaseEntity
{
    private Permission() { }  // EF Core only

    /// <summary>
    /// Creates a new permission with resource.action pattern
    /// </summary>
    /// <param name="name">Permission name (e.g., "create_product")</param>
    /// <param name="resource">Resource name (e.g., "product", "inventory")</param>
    /// <param name="action">Action name (e.g., "create", "read", "update", "delete")</param>
    /// <param name="description">Optional description</param>
    public Permission(string name, string resource, string action, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Permission name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(resource)) throw new ArgumentException("Resource is required", nameof(resource));
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action is required", nameof(action));
        
        Id = Guid.NewGuid();
        Name = name;
        Resource = resource;
        Action = action;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; } = null!;  // e.g., "create_product"
    public string? Description { get; private set; }
    public string Resource { get; private set; } = null!;  // e.g., "product", "inventory"
    public string Action { get; private set; } = null!;  // e.g., "create", "read", "update", "delete"
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; } = new List<RolePermission>();

    /// <summary>
    /// Updates permission information
    /// </summary>
    public void Update(string name, string resource, string action, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Permission name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(resource)) throw new ArgumentException("Resource is required", nameof(resource));
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action is required", nameof(action));
        
        Name = name;
        Resource = resource;
        Action = action;
        Description = description;
    }
}

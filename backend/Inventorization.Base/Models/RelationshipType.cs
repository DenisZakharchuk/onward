namespace Inventorization.Base.Models;

/// <summary>
/// Defines the type of relationship between entities
/// </summary>
public enum RelationshipType
{
    /// <summary>
    /// One-to-one relationship (1:1)
    /// Example: User ↔ UserProfile
    /// </summary>
    OneToOne,

    /// <summary>
    /// One-to-many relationship (1:N)
    /// Example: User → RefreshTokens, Category → Products
    /// </summary>
    OneToMany,

    /// <summary>
    /// Many-to-many relationship (M:N) via junction entity
    /// Example: User ↔ Role (via UserRole), Role ↔ Permission (via RolePermission)
    /// </summary>
    ManyToMany
}

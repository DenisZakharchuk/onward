using Inventorization.Base.Abstractions;
using Inventorization.Base.Models;
using Inventorization.Auth.Domain.Entities;

namespace Inventorization.Auth.Domain;

/// <summary>
/// Centralized repository of all relationship metadata for the Auth bounded context.
/// Single source of truth for entity relationships - used by relationship managers,
/// configurations, and DI registration.
/// </summary>
public static class DataModelRelationships
{
    /// <summary>
    /// User ↔ Role many-to-many relationship via UserRole junction
    /// </summary>
    public static readonly IRelationshipMetadata<User, Role> UserRoles =
        new RelationshipMetadata<User, Role>(
            type: RelationshipType.ManyToMany,
            cardinality: RelationshipCardinality.Optional,
            entityName: nameof(User),
            relatedEntityName: nameof(Role),
            displayName: "User Roles",
            junctionEntityName: nameof(UserRole),
            navigationPropertyName: nameof(User.UserRoles),
            description: "Manages the many-to-many relationship between users and their assigned roles");

    /// <summary>
    /// Role ↔ Permission many-to-many relationship via RolePermission junction
    /// </summary>
    public static readonly IRelationshipMetadata<Role, Permission> RolePermissions =
        new RelationshipMetadata<Role, Permission>(
            type: RelationshipType.ManyToMany,
            cardinality: RelationshipCardinality.Optional,
            entityName: nameof(Role),
            relatedEntityName: nameof(Permission),
            displayName: "Role Permissions",
            junctionEntityName: nameof(RolePermission),
            navigationPropertyName: nameof(Role.RolePermissions),
            description: "Manages the many-to-many relationship between roles and their assigned permissions");

    /// <summary>
    /// User → RefreshToken one-to-many relationship
    /// </summary>
    public static readonly IRelationshipMetadata<User, RefreshToken> UserRefreshTokens =
        new RelationshipMetadata<User, RefreshToken>(
            type: RelationshipType.OneToMany,
            cardinality: RelationshipCardinality.Required,
            entityName: nameof(User),
            relatedEntityName: nameof(RefreshToken),
            displayName: "User Refresh Tokens",
            navigationPropertyName: nameof(User.RefreshTokens),
            description: "Manages the one-to-many relationship between users and their refresh tokens");
}

using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Base.Models;
using Inventorization.Auth.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Inventorization.Auth.Domain.DataServices;

/// <summary>
/// Manages User-Role relationships with add/remove semantics.
/// Property accessors are resolved from DI container.
/// </summary>
public class UserRoleRelationshipManager : RelationshipManagerBase<User, Role, UserRole>
{
    public UserRoleRelationshipManager(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<UserRoleRelationshipManager> logger)
        : base(
            userRepository, 
            roleRepository, 
            userRoleRepository, 
            unitOfWork, 
            serviceProvider, 
            logger,
            new RelationshipMetadata(
                type: RelationshipType.ManyToMany,
                cardinality: RelationshipCardinality.Optional,
                entityName: nameof(User),
                relatedEntityName: nameof(Role),
                displayName: "User Roles",
                junctionEntityName: nameof(UserRole),
                description: "Manages the many-to-many relationship between users and their assigned roles"))
    {
    }
}

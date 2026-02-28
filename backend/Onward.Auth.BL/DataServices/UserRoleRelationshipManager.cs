using Onward.Base.DataAccess;
using Onward.Base.Services;
using Onward.Base.Abstractions;
using Onward.Auth.BL.Entities;
using Microsoft.Extensions.Logging;

namespace Onward.Auth.BL.DataServices;

/// <summary>
/// Manages User-Role relationships with add/remove semantics.
/// Property accessors and metadata are resolved from DI container.
/// </summary>
public class UserRoleRelationshipManager : RelationshipManagerBase<User, Role, UserRole>
{
    public UserRoleRelationshipManager(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<UserRoleRelationshipManager> logger,
        IRelationshipMetadata<User, Role> metadata)
        : base(
            userRepository, 
            roleRepository, 
            userRoleRepository, 
            unitOfWork, 
            serviceProvider, 
            logger,
            metadata)
    {
    }
}

using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
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
        : base(userRepository, roleRepository, userRoleRepository, unitOfWork, serviceProvider, logger)
    {
    }
}

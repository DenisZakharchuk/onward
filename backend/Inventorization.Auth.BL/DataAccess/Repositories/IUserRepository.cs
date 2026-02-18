using Inventorization.Auth.BL.Entities;
using Inventorization.Base.DataAccess;

namespace Inventorization.Auth.BL.DataAccess.Repositories;

/// <summary>
/// Repository interface for User entity with domain-specific queries
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by email address
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user with their roles included
    /// </summary>
    Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user with their roles and permissions included
    /// </summary>
    Task<User?> GetUserWithPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID (alias for GetByIdAsync for clarity)
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

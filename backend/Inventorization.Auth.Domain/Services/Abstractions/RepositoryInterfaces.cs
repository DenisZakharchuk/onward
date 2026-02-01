using Inventorization.Auth.Domain.Entities;
using Inventorization.Base.DataAccess;

namespace Inventorization.Auth.Domain.Services.Abstractions;

/// <summary>
/// Password hasher abstraction (BCrypt, Argon2, etc.)
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// Refresh token repository with specialized methods
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenValueAsync(string tokenValue, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetTokensByFamilyAsync(string family, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// User repository with specialized methods
/// </summary>
public interface IUserRepository : IRepository<Entities.User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

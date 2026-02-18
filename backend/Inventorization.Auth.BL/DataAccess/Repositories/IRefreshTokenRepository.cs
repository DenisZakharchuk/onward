using Inventorization.Auth.BL.Entities;
using Inventorization.Base.DataAccess;

namespace Inventorization.Auth.BL.DataAccess.Repositories;

/// <summary>
/// Repository interface for RefreshToken entity with token-specific queries
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Gets a refresh token by its token value
    /// </summary>
    Task<RefreshToken?> GetByTokenValueAsync(string tokenValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all refresh tokens in a family (for reuse detection)
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetTokensByFamilyAsync(string family, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active (non-revoked) refresh tokens for a user
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

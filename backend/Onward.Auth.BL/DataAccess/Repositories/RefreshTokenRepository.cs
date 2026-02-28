using Onward.Auth.BL.Entities;
using Onward.Auth.BL.Services.Abstractions;
using Onward.Auth.BL.DbContexts;
using Onward.Base.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Onward.Auth.BL.DataAccess.Repositories;

/// <summary>
/// Refresh token repository implementation
/// </summary>
public class RefreshTokenRepository : BaseRepository<RefreshToken>, Services.Abstractions.IRefreshTokenRepository
{
    private readonly AuthDbContext _context;

    public RefreshTokenRepository(AuthDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenValueAsync(string tokenValue, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == tokenValue, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetTokensByFamilyAsync(string family, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.Family == family)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}

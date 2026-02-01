using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.Domain.Services.Abstractions;
using Inventorization.Auth.Domain.DbContexts;
using Inventorization.Base.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Inventorization.Auth.Domain.DataAccess.Repositories;

/// <summary>
/// User repository implementation
/// </summary>
public class UserRepository : BaseRepository<User>, 
    Services.Abstractions.IUserRepository,
    Inventorization.Auth.Domain.DataAccess.Repositories.IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetUserWithPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(userId, cancellationToken);
    }
}

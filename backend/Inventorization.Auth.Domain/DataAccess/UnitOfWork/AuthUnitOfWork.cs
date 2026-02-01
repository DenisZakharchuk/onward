using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.Domain.DataAccess.Repositories;
using Inventorization.Base.DataAccess;
using Microsoft.Extensions.Logging;
using Inventorization.Auth.Domain.DbContexts;
using IUserRepository = Inventorization.Auth.Domain.Services.Abstractions.IUserRepository;
using IRefreshTokenRepository = Inventorization.Auth.Domain.Services.Abstractions.IRefreshTokenRepository;

namespace Inventorization.Auth.Domain.DataAccess.UnitOfWork;

/// <summary>
/// Unit of Work implementation coordinating all Auth repositories and transactions
/// </summary>
public class AuthUnitOfWork : IAuthUnitOfWork
{
    private readonly AuthDbContext _context;
    private readonly ILogger<AuthUnitOfWork> _logger;

    public AuthUnitOfWork(AuthDbContext context, ILogger<AuthUnitOfWork> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Saves all changes made in repositories to the database
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            int changes = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Saved {ChangeCount} changes to the database", changes);
            return changes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction != null)
            throw new InvalidOperationException("Transaction already in progress");

        await _context.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogDebug("Database transaction started");
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_context.Database.CurrentTransaction == null)
                throw new InvalidOperationException("No transaction in progress");

            await _context.Database.CommitTransactionAsync(cancellationToken);
            _logger.LogDebug("Database transaction committed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction");
            throw;
        }
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_context.Database.CurrentTransaction == null)
                throw new InvalidOperationException("No transaction in progress");

            await _context.Database.RollbackTransactionAsync(cancellationToken);
            _logger.LogDebug("Database transaction rolled back");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back transaction");
            throw;
        }
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_context.Database.CurrentTransaction != null)
        {
            await _context.Database.RollbackTransactionAsync();
            _logger.LogWarning("Disposed UnitOfWork with active transaction - rolled back");
        }

        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources synchronously
    /// </summary>
    public void Dispose()
    {
        if (_context.Database.CurrentTransaction != null)
        {
            _context.Database.RollbackTransaction();
            _logger.LogWarning("Disposed UnitOfWork with active transaction - rolled back");
        }

        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

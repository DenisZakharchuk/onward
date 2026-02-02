using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventorization.Base.DataAccess;

/// <summary>
/// Generic base class for Unit of Work pattern implementation.
/// Provides transaction management, change tracking, and disposal for Entity Framework DbContext.
/// All methods are virtual to allow bounded contexts to override for specialized behavior.
/// </summary>
/// <typeparam name="TDbContext">The Entity Framework DbContext type for the bounded context</typeparam>
public abstract class UnitOfWorkBase<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    protected readonly TDbContext Context;
    protected readonly ILogger<UnitOfWorkBase<TDbContext>> Logger;

    protected UnitOfWorkBase(TDbContext context, ILogger<UnitOfWorkBase<TDbContext>> logger)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Saves all changes made to tracked entities
    /// </summary>
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            int changes = await Context.SaveChangesAsync(cancellationToken);
            Logger.LogDebug("Saved {ChangeCount} changes to the database", changes);
            return changes;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Database.CurrentTransaction != null)
            throw new InvalidOperationException("Transaction already in progress");

        await Context.Database.BeginTransactionAsync(cancellationToken);
        Logger.LogDebug("Database transaction started");
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (Context.Database.CurrentTransaction == null)
                throw new InvalidOperationException("No transaction in progress");

            await Context.Database.CommitTransactionAsync(cancellationToken);
            Logger.LogDebug("Database transaction committed");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error committing transaction");
            throw;
        }
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (Context.Database.CurrentTransaction == null)
                throw new InvalidOperationException("No transaction in progress");

            await Context.Database.RollbackTransactionAsync(cancellationToken);
            Logger.LogDebug("Database transaction rolled back");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error rolling back transaction");
            throw;
        }
    }

    /// <summary>
    /// Disposes resources asynchronously
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        if (Context.Database.CurrentTransaction != null)
        {
            await Context.Database.RollbackTransactionAsync();
            Logger.LogWarning("Disposed UnitOfWork with active transaction - rolled back");
        }

        await Context.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources synchronously
    /// </summary>
    public virtual void Dispose()
    {
        if (Context.Database.CurrentTransaction != null)
        {
            Context.Database.RollbackTransaction();
            Logger.LogWarning("Disposed UnitOfWork with active transaction - rolled back");
        }

        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}

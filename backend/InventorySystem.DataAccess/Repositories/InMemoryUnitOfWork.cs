using InventorySystem.DataAccess.Abstractions;

namespace InventorySystem.DataAccess.Repositories;

/// <summary>
/// Placeholder in-memory implementation of Unit of Work.
/// </summary>
public class InMemoryUnitOfWork : IUnitOfWork
{
    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }
    public IStockMovementRepository StockMovements { get; }

    public InMemoryUnitOfWork(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IStockMovementRepository stockMovementRepository)
    {
        Products = productRepository;
        Categories = categoryRepository;
        StockMovements = stockMovementRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In-memory implementation doesn't need to persist changes
        return Task.FromResult(0);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // No-op for in-memory implementation
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        // No-op for in-memory implementation
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        // No-op for in-memory implementation
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Nothing to dispose in in-memory implementation
    }
}

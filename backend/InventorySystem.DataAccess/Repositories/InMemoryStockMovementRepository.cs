using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;

namespace InventorySystem.DataAccess.Repositories;

/// <summary>
/// Placeholder in-memory implementation of StockMovement repository.
/// </summary>
public class InMemoryStockMovementRepository : IStockMovementRepository
{
    private readonly List<StockMovement> _movements = new();

    public Task<StockMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movement = _movements.FirstOrDefault(m => m.Id == id);
        return Task.FromResult(movement);
    }

    public Task<IEnumerable<StockMovement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<StockMovement>>(_movements.ToList());
    }

    public Task<StockMovement> CreateAsync(StockMovement entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        _movements.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<StockMovement> UpdateAsync(StockMovement entity, CancellationToken cancellationToken = default)
    {
        var existing = _movements.FirstOrDefault(m => m.Id == entity.Id);
        if (existing != null)
        {
            _movements.Remove(existing);
            _movements.Add(entity);
        }
        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movement = _movements.FirstOrDefault(m => m.Id == id);
        if (movement != null)
        {
            _movements.Remove(movement);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<IEnumerable<StockMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var movements = _movements.Where(m => m.ProductId == productId).OrderByDescending(m => m.CreatedAt).ToList();
        return Task.FromResult<IEnumerable<StockMovement>>(movements);
    }

    public Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var movements = _movements.Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate).ToList();
        return Task.FromResult<IEnumerable<StockMovement>>(movements);
    }
}

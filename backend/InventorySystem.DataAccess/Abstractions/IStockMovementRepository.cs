using InventorySystem.DataAccess.Models;

namespace InventorySystem.DataAccess.Abstractions;

/// <summary>
/// Stock movement repository interface for tracking inventory changes
/// </summary>
public interface IStockMovementRepository : IRepository<StockMovement>
{
    Task<IEnumerable<StockMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

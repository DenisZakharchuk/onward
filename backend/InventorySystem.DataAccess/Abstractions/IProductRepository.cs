using InventorySystem.DataAccess.Models;

namespace InventorySystem.DataAccess.Abstractions;

/// <summary>
/// Product-specific repository interface extending the generic repository
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<bool> UpdateStockAsync(Guid productId, int newStock, CancellationToken cancellationToken = default);
}

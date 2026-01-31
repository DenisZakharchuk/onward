using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;

namespace InventorySystem.DataAccess.Repositories;

/// <summary>
/// Placeholder in-memory implementation of Product repository.
/// This will be replaced with actual storage implementation (SQL, NoSQL, etc.) in next steps.
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Product>>(_products.ToList());
    }

    public Task<Product> CreateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        _products.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        var existing = _products.FirstOrDefault(p => p.Id == entity.Id);
        if (existing != null)
        {
            _products.Remove(existing);
            entity.UpdatedAt = DateTime.UtcNow;
            _products.Add(entity);
        }
        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var products = _products.Where(p => p.CategoryId == categoryId).ToList();
        return Task.FromResult<IEnumerable<Product>>(products);
    }

    public Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = _products.Where(p => p.CurrentStock <= p.MinimumStock).ToList();
        return Task.FromResult<IEnumerable<Product>>(products);
    }

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var product = _products.FirstOrDefault(p => p.SKU == sku);
        return Task.FromResult(product);
    }

    public Task<bool> UpdateStockAsync(Guid productId, int newStock, CancellationToken cancellationToken = default)
    {
        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product != null)
        {
            product.CurrentStock = newStock;
            product.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public IQueryable<Product> GetQueryable()
    {
        return _products.AsQueryable();
    }
}

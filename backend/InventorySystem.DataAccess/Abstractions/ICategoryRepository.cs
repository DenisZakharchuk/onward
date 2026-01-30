using InventorySystem.DataAccess.Models;

namespace InventorySystem.DataAccess.Abstractions;

/// <summary>
/// Category-specific repository interface
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

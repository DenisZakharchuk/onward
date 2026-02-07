using System.Linq.Expressions;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;

namespace InventorySystem.DataAccess.Repositories;

/// <summary>
/// Placeholder in-memory implementation of Category repository.
/// </summary>
public class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories = new();

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(category);
    }

    public Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Category>>(_categories.ToList());
    }

    public Task<Category> CreateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        _categories.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<Category> UpdateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        var existing = _categories.FirstOrDefault(c => c.Id == entity.Id);
        if (existing != null)
        {
            _categories.Remove(existing);
            entity.UpdatedAt = DateTime.UtcNow;
            _categories.Add(entity);
        }
        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category != null)
        {
            _categories.Remove(category);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var category = _categories.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(category);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = _categories.Any(c => c.Id == id);
        return Task.FromResult(exists);
    }

    public Task<IEnumerable<Category>> FindAsync(Expression<Func<Category, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var results = _categories.AsQueryable().Where(predicate).ToList();
        return Task.FromResult<IEnumerable<Category>>(results);
    }

    public IQueryable<Category> GetQueryable()
    {
        return _categories.AsQueryable();
    }
}

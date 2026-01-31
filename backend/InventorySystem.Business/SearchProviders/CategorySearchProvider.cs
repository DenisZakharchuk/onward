using System.Linq.Expressions;
using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Category;

namespace InventorySystem.Business.SearchProviders;

/// <summary>
/// Creates search expressions for Category entities
/// </summary>
public class CategorySearchProvider : ISearchQueryProvider<Category, CategorySearchDTO>
{
    public Expression<Func<Category, bool>> GetSearchExpression(CategorySearchDTO searchDto)
    {
        return c =>
            string.IsNullOrEmpty(searchDto.NameFilter) || c.Name.Contains(searchDto.NameFilter);
    }
}

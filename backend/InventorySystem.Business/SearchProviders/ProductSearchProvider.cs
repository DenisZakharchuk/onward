using System.Linq.Expressions;
using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Product;

namespace InventorySystem.Business.SearchProviders;

/// <summary>
/// Creates search expressions for Product entities
/// </summary>
public class ProductSearchProvider : ISearchQueryProvider<Product, ProductSearchDTO>
{
    public Expression<Func<Product, bool>> GetSearchExpression(ProductSearchDTO searchDto)
    {
        return p =>
            (string.IsNullOrEmpty(searchDto.NameFilter) || p.Name.Contains(searchDto.NameFilter)) &&
            (!searchDto.MinPrice.HasValue || p.Price >= searchDto.MinPrice.Value) &&
            (!searchDto.MaxPrice.HasValue || p.Price <= searchDto.MaxPrice.Value) &&
            (!searchDto.CategoryId.HasValue || p.CategoryId == searchDto.CategoryId) &&
            (!searchDto.LowStockOnly.HasValue || !searchDto.LowStockOnly.Value || p.CurrentStock <= p.MinimumStock);
    }
}

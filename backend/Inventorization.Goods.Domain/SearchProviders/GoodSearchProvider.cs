using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Good;

namespace Inventorization.Goods.Domain.SearchProviders;

/// <summary>
/// Provides search query expressions for Good entities
/// </summary>
public class GoodSearchProvider : ISearchQueryProvider<Good, GoodSearchDTO>
{
    public Expression<Func<Good, bool>> GetSearchExpression(GoodSearchDTO searchDto)
    {
        if (searchDto == null) throw new ArgumentNullException(nameof(searchDto));
        
        return entity =>
            (string.IsNullOrEmpty(searchDto.NameFilter) || entity.Name.Contains(searchDto.NameFilter)) &&
            (string.IsNullOrEmpty(searchDto.SkuFilter) || entity.Sku.Contains(searchDto.SkuFilter)) &&
            (!searchDto.MinPrice.HasValue || entity.UnitPrice >= searchDto.MinPrice.Value) &&
            (!searchDto.MaxPrice.HasValue || entity.UnitPrice <= searchDto.MaxPrice.Value) &&
            (!searchDto.IsActiveFilter.HasValue || entity.IsActive == searchDto.IsActiveFilter.Value) &&
            (!searchDto.MinQuantity.HasValue || entity.QuantityInStock >= searchDto.MinQuantity.Value) &&
            (!searchDto.MaxQuantity.HasValue || entity.QuantityInStock <= searchDto.MaxQuantity.Value);
    }
}

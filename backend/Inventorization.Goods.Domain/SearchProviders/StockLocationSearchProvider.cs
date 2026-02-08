using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.StockLocation;

namespace Inventorization.Goods.Domain.SearchProviders;

/// <summary>
/// Provides search query expressions for StockLocation entities
/// </summary>
public class StockLocationSearchProvider : ISearchQueryProvider<StockLocation, StockLocationSearchDTO>
{
    public Expression<Func<StockLocation, bool>> GetSearchExpression(StockLocationSearchDTO searchDto)
    {
        if (searchDto == null) throw new ArgumentNullException(nameof(searchDto));
        
        return entity =>
            (!searchDto.WarehouseId.HasValue || entity.WarehouseId == searchDto.WarehouseId.Value) &&
            (string.IsNullOrEmpty(searchDto.Code) || entity.Code.Contains(searchDto.Code)) &&
            (string.IsNullOrEmpty(searchDto.Aisle) || (entity.Aisle != null && entity.Aisle.Contains(searchDto.Aisle))) &&
            (!searchDto.IsActive.HasValue || entity.IsActive == searchDto.IsActive.Value);
    }
}

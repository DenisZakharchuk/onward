using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.StockItem;

namespace Inventorization.Goods.BL.SearchProviders;

/// <summary>
/// Provides search query expressions for StockItem entities
/// </summary>
public class StockItemSearchProvider : ISearchQueryProvider<StockItem, StockItemSearchDTO>
{
    public Expression<Func<StockItem, bool>> GetSearchExpression(StockItemSearchDTO searchDto)
    {
        if (searchDto == null) throw new ArgumentNullException(nameof(searchDto));
        
        return entity =>
            (!searchDto.GoodId.HasValue || entity.GoodId == searchDto.GoodId.Value) &&
            (!searchDto.StockLocationId.HasValue || entity.StockLocationId == searchDto.StockLocationId.Value) &&
            (string.IsNullOrEmpty(searchDto.BatchNumber) || (entity.BatchNumber != null && entity.BatchNumber.Contains(searchDto.BatchNumber))) &&
            (string.IsNullOrEmpty(searchDto.SerialNumber) || (entity.SerialNumber != null && entity.SerialNumber.Contains(searchDto.SerialNumber))) &&
            (!searchDto.ExpiryDateFrom.HasValue || (entity.ExpiryDate.HasValue && entity.ExpiryDate.Value >= searchDto.ExpiryDateFrom.Value)) &&
            (!searchDto.ExpiryDateTo.HasValue || (entity.ExpiryDate.HasValue && entity.ExpiryDate.Value <= searchDto.ExpiryDateTo.Value));
    }
}

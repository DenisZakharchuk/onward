using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.StockItem;

namespace Inventorization.Goods.BL.Creators;

/// <summary>
/// Creates StockItem entities from CreateStockItemDTO
/// </summary>
public class StockItemCreator : IEntityCreator<StockItem, CreateStockItemDTO>
{
    public StockItem Create(CreateStockItemDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var stockItem = new StockItem(
            goodId: dto.GoodId,
            stockLocationId: dto.StockLocationId,
            quantity: dto.Quantity
        );
        
        // Update optional tracking information
        if (dto.BatchNumber != null || dto.SerialNumber != null || dto.ExpiryDate.HasValue)
        {
            stockItem.UpdateTrackingInfo(
                batchNumber: dto.BatchNumber,
                serialNumber: dto.SerialNumber,
                expiryDate: dto.ExpiryDate
            );
        }
        
        return stockItem;
    }
}

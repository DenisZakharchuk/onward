using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.StockItem;

namespace Inventorization.Goods.Domain.Modifiers;

/// <summary>
/// Updates StockItem entities from UpdateStockItemDTO
/// </summary>
public class StockItemModifier : IEntityModifier<StockItem, UpdateStockItemDTO>
{
    public void Modify(StockItem entity, UpdateStockItemDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        // Update quantity if changed
        if (entity.Quantity != dto.Quantity)
        {
            entity.UpdateQuantity(dto.Quantity);
        }
        
        // Update location if changed
        if (entity.StockLocationId != dto.StockLocationId)
        {
            entity.MoveToLocation(dto.StockLocationId);
        }
        
        // Update tracking information
        entity.UpdateTrackingInfo(
            batchNumber: dto.BatchNumber,
            serialNumber: dto.SerialNumber,
            expiryDate: dto.ExpiryDate
        );
    }
}

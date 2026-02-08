using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.StockLocation;

namespace Inventorization.Goods.Domain.Modifiers;

/// <summary>
/// Updates StockLocation entities from UpdateStockLocationDTO
/// </summary>
public class StockLocationModifier : IEntityModifier<StockLocation, UpdateStockLocationDTO>
{
    public void Modify(StockLocation entity, UpdateStockLocationDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        // Update warehouse if changed
        if (entity.WarehouseId != dto.WarehouseId)
        {
            entity.UpdateWarehouse(dto.WarehouseId);
        }
        
        // Update all other properties using the Update method
        entity.Update(
            code: dto.Code,
            aisle: dto.Aisle,
            shelf: dto.Shelf,
            bin: dto.Bin,
            description: dto.Description
        );
    }
}

using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.StockLocation;

namespace Inventorization.Goods.Domain.Creators;

/// <summary>
/// Creates StockLocation entities from CreateStockLocationDTO
/// </summary>
public class StockLocationCreator : IEntityCreator<StockLocation, CreateStockLocationDTO>
{
    public StockLocation Create(CreateStockLocationDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var stockLocation = new StockLocation(
            warehouseId: dto.WarehouseId,
            code: dto.Code
        );
        
        // Update optional properties using the Update method
        stockLocation.Update(
            code: dto.Code,
            aisle: dto.Aisle,
            shelf: dto.Shelf,
            bin: dto.Bin,
            description: dto.Description
        );
        
        return stockLocation;
    }
}

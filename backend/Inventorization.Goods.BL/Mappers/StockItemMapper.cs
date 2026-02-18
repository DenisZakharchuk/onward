using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.StockItem;

namespace Inventorization.Goods.BL.Mappers;

/// <summary>
/// Maps StockItem entities to StockItemDetailsDTO
/// </summary>
public class StockItemMapper : IMapper<StockItem, StockItemDetailsDTO>
{
    public StockItemDetailsDTO Map(StockItem entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        return new StockItemDetailsDTO
        {
            Id = entity.Id,
            GoodId = entity.GoodId,
            StockLocationId = entity.StockLocationId,
            Quantity = entity.Quantity,
            BatchNumber = entity.BatchNumber,
            SerialNumber = entity.SerialNumber,
            ExpiryDate = entity.ExpiryDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    public Expression<Func<StockItem, StockItemDetailsDTO>> GetProjection()
    {
        return entity => new StockItemDetailsDTO
        {
            Id = entity.Id,
            GoodId = entity.GoodId,
            StockLocationId = entity.StockLocationId,
            Quantity = entity.Quantity,
            BatchNumber = entity.BatchNumber,
            SerialNumber = entity.SerialNumber,
            ExpiryDate = entity.ExpiryDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

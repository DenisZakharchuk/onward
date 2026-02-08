using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.StockLocation;

namespace Inventorization.Goods.Domain.Mappers;

/// <summary>
/// Maps StockLocation entities to StockLocationDetailsDTO
/// </summary>
public class StockLocationMapper : IMapper<StockLocation, StockLocationDetailsDTO>
{
    public StockLocationDetailsDTO Map(StockLocation entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        return new StockLocationDetailsDTO
        {
            Id = entity.Id,
            WarehouseId = entity.WarehouseId,
            Code = entity.Code,
            Aisle = entity.Aisle,
            Shelf = entity.Shelf,
            Bin = entity.Bin,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    public Expression<Func<StockLocation, StockLocationDetailsDTO>> GetProjection()
    {
        return entity => new StockLocationDetailsDTO
        {
            Id = entity.Id,
            WarehouseId = entity.WarehouseId,
            Code = entity.Code,
            Aisle = entity.Aisle,
            Shelf = entity.Shelf,
            Bin = entity.Bin,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

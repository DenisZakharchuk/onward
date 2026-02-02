using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Good;

namespace Inventorization.Goods.Domain.Mappers;

/// <summary>
/// Maps Good entities to GoodDetailsDTO
/// </summary>
public class GoodMapper : IMapper<Good, GoodDetailsDTO>
{
    public GoodDetailsDTO Map(Good entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        return new GoodDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Sku = entity.Sku,
            UnitPrice = entity.UnitPrice,
            QuantityInStock = entity.QuantityInStock,
            UnitOfMeasure = entity.UnitOfMeasure,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    public Expression<Func<Good, GoodDetailsDTO>> GetProjection()
    {
        return entity => new GoodDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Sku = entity.Sku,
            UnitPrice = entity.UnitPrice,
            QuantityInStock = entity.QuantityInStock,
            UnitOfMeasure = entity.UnitOfMeasure,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.PurchaseOrderItem;

namespace Inventorization.Goods.Domain.Mappers;

/// <summary>
/// Maps PurchaseOrderItem entities to PurchaseOrderItemDetailsDTO
/// </summary>
public class PurchaseOrderItemMapper : IMapper<PurchaseOrderItem, PurchaseOrderItemDetailsDTO>
{
    public PurchaseOrderItemDetailsDTO Map(PurchaseOrderItem entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        return new PurchaseOrderItemDetailsDTO
        {
            Id = entity.Id,
            PurchaseOrderId = entity.PurchaseOrderId,
            GoodId = entity.GoodId,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            ReceivedQuantity = entity.ReceivedQuantity,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    public Expression<Func<PurchaseOrderItem, PurchaseOrderItemDetailsDTO>> GetProjection()
    {
        return entity => new PurchaseOrderItemDetailsDTO
        {
            Id = entity.Id,
            PurchaseOrderId = entity.PurchaseOrderId,
            GoodId = entity.GoodId,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            ReceivedQuantity = entity.ReceivedQuantity,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

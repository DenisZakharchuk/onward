using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.PurchaseOrder;

namespace Inventorization.Goods.BL.Mappers;

/// <summary>
/// Maps PurchaseOrder entities to PurchaseOrderDetailsDTO
/// </summary>
public class PurchaseOrderMapper : IMapper<PurchaseOrder, PurchaseOrderDetailsDTO>
{
    public PurchaseOrderDetailsDTO Map(PurchaseOrder entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        return new PurchaseOrderDetailsDTO
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            SupplierId = entity.SupplierId,
            OrderDate = entity.OrderDate,
            ExpectedDeliveryDate = entity.ExpectedDeliveryDate,
            ActualDeliveryDate = entity.ActualDeliveryDate,
            Status = entity.Status,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    public Expression<Func<PurchaseOrder, PurchaseOrderDetailsDTO>> GetProjection()
    {
        return entity => new PurchaseOrderDetailsDTO
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            SupplierId = entity.SupplierId,
            OrderDate = entity.OrderDate,
            ExpectedDeliveryDate = entity.ExpectedDeliveryDate,
            ActualDeliveryDate = entity.ActualDeliveryDate,
            Status = entity.Status,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

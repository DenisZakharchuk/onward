using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.PurchaseOrder;

namespace Inventorization.Goods.Domain.Modifiers;

/// <summary>
/// Updates PurchaseOrder entities from UpdatePurchaseOrderDTO
/// </summary>
public class PurchaseOrderModifier : IEntityModifier<PurchaseOrder, UpdatePurchaseOrderDTO>
{
    public void Modify(PurchaseOrder entity, UpdatePurchaseOrderDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        // Use the entity's Update method to maintain immutability pattern
        entity.Update(
            orderNumber: dto.OrderNumber,
            orderDate: dto.OrderDate,
            expectedDeliveryDate: dto.ExpectedDeliveryDate,
            notes: dto.Notes
        );
    }
}

using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.PurchaseOrderItem;

namespace Inventorization.Goods.BL.Modifiers;

/// <summary>
/// Updates PurchaseOrderItem entities from UpdatePurchaseOrderItemDTO
/// </summary>
public class PurchaseOrderItemModifier : IEntityModifier<PurchaseOrderItem, UpdatePurchaseOrderItemDTO>
{
    public void Modify(PurchaseOrderItem entity, UpdatePurchaseOrderItemDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        // Use the entity's Update method to maintain immutability pattern
        entity.Update(
            quantity: dto.Quantity,
            unitPrice: dto.UnitPrice,
            notes: dto.Notes
        );
    }
}

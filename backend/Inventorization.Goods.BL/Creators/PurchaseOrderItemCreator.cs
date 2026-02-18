using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.PurchaseOrderItem;

namespace Inventorization.Goods.BL.Creators;

/// <summary>
/// Creates PurchaseOrderItem entities from CreatePurchaseOrderItemDTO
/// </summary>
public class PurchaseOrderItemCreator : IEntityCreator<PurchaseOrderItem, CreatePurchaseOrderItemDTO>
{
    public PurchaseOrderItem Create(CreatePurchaseOrderItemDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var purchaseOrderItem = new PurchaseOrderItem(
            purchaseOrderId: dto.PurchaseOrderId,
            goodId: dto.GoodId,
            quantity: dto.Quantity,
            unitPrice: dto.UnitPrice
        );
        
        // Update optional notes using the Update method
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            purchaseOrderItem.Update(
                quantity: dto.Quantity,
                unitPrice: dto.UnitPrice,
                notes: dto.Notes
            );
        }
        
        return purchaseOrderItem;
    }
}

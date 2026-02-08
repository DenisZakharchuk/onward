using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.PurchaseOrder;

namespace Inventorization.Goods.Domain.Creators;

/// <summary>
/// Creates PurchaseOrder entities from CreatePurchaseOrderDTO
/// </summary>
public class PurchaseOrderCreator : IEntityCreator<PurchaseOrder, CreatePurchaseOrderDTO>
{
    public PurchaseOrder Create(CreatePurchaseOrderDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var purchaseOrder = new PurchaseOrder(
            orderNumber: dto.OrderNumber,
            supplierId: dto.SupplierId,
            orderDate: dto.OrderDate
        );
        
        // Update optional properties using the Update method
        purchaseOrder.Update(
            orderNumber: dto.OrderNumber,
            orderDate: dto.OrderDate,
            expectedDeliveryDate: dto.ExpectedDeliveryDate,
            notes: dto.Notes
        );
        
        return purchaseOrder;
    }
}

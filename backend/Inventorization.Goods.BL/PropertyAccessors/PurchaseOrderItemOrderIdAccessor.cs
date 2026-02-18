using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.PropertyAccessors;

/// <summary>
/// Property accessor for PurchaseOrderItem.PurchaseOrderId
/// Used for managing PurchaseOrderItem-to-PurchaseOrder relationship
/// </summary>
public class PurchaseOrderItemOrderIdAccessor : PropertyAccessor<PurchaseOrderItem, Guid>
{
    public PurchaseOrderItemOrderIdAccessor() 
        : base(item => item.PurchaseOrderId)
    {
    }
}

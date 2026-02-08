using Inventorization.Goods.Domain.Entities;

namespace Inventorization.Goods.Domain.PropertyAccessors;

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

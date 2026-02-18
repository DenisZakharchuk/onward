using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.PropertyAccessors;

/// <summary>
/// Property accessor for PurchaseOrderItem.GoodId
/// Used for managing PurchaseOrderItem-to-Good relationship
/// </summary>
public class PurchaseOrderItemGoodIdAccessor : PropertyAccessor<PurchaseOrderItem, Guid>
{
    public PurchaseOrderItemGoodIdAccessor() 
        : base(item => item.GoodId)
    {
    }
}

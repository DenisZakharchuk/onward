using Inventorization.Goods.Domain.Entities;

namespace Inventorization.Goods.Domain.PropertyAccessors;

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

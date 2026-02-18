using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.PropertyAccessors;

/// <summary>
/// Property accessor for PurchaseOrder.SupplierId
/// Used for managing PurchaseOrder-to-Supplier relationship
/// </summary>
public class PurchaseOrderSupplierIdAccessor : PropertyAccessor<PurchaseOrder, Guid>
{
    public PurchaseOrderSupplierIdAccessor() 
        : base(purchaseOrder => purchaseOrder.SupplierId)
    {
    }
}

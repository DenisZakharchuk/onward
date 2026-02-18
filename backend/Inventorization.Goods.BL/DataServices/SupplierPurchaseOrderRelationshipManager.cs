using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.BL.PropertyAccessors;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.BL.DataServices;

/// <summary>
/// Manages Supplier → PurchaseOrder one-to-many relationships.
/// A supplier can have multiple purchase orders.
/// </summary>
public class SupplierPurchaseOrderRelationshipManager : OneToManyRelationshipManagerBase<Supplier, PurchaseOrder>
{
    public SupplierPurchaseOrderRelationshipManager(
        IRepository<Supplier> parentRepository,
        IRepository<PurchaseOrder> childRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<SupplierPurchaseOrderRelationshipManager> logger)
        : base(parentRepository, childRepository, unitOfWork, serviceProvider, logger,
               DataModelRelationships.SupplierPurchaseOrders, typeof(PurchaseOrderSupplierIdAccessor))
    {
    }

    protected override void SetParentId(PurchaseOrder child, Guid? parentId)
    {
        var property = typeof(PurchaseOrder).GetProperty("SupplierId");
        property?.SetValue(child, parentId ?? Guid.Empty);
    }
}

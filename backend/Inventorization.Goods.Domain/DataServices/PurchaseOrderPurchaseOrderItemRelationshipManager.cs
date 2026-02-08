using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.Domain.PropertyAccessors;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.Domain.DataServices;

/// <summary>
/// Manages PurchaseOrder → PurchaseOrderItem one-to-many relationships.
/// A purchase order can contain multiple line items.
/// </summary>
public class PurchaseOrderPurchaseOrderItemRelationshipManager : OneToManyRelationshipManagerBase<PurchaseOrder, PurchaseOrderItem>
{
    public PurchaseOrderPurchaseOrderItemRelationshipManager(
        IRepository<PurchaseOrder> parentRepository,
        IRepository<PurchaseOrderItem> childRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<PurchaseOrderPurchaseOrderItemRelationshipManager> logger)
        : base(parentRepository, childRepository, unitOfWork, serviceProvider, logger,
               DataModelRelationships.PurchaseOrderItems, typeof(PurchaseOrderItemOrderIdAccessor))
    {
    }

    protected override void SetParentId(PurchaseOrderItem child, Guid? parentId)
    {
        var property = typeof(PurchaseOrderItem).GetProperty("PurchaseOrderId");
        property?.SetValue(child, parentId ?? Guid.Empty);
    }
}

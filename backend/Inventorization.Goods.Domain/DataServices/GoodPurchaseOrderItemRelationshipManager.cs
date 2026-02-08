using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.Domain.PropertyAccessors;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.Domain.DataServices;

/// <summary>
/// Manages Good → PurchaseOrderItem one-to-many relationships.
/// A good can appear in multiple purchase order line items.
/// </summary>
public class GoodPurchaseOrderItemRelationshipManager : OneToManyRelationshipManagerBase<Good, PurchaseOrderItem>
{
    public GoodPurchaseOrderItemRelationshipManager(
        IRepository<Good> parentRepository,
        IRepository<PurchaseOrderItem> childRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<GoodPurchaseOrderItemRelationshipManager> logger)
        : base(parentRepository, childRepository, unitOfWork, serviceProvider, logger,
               DataModelRelationships.GoodPurchaseOrderItems, typeof(PurchaseOrderItemGoodIdAccessor))
    {
    }

    protected override void SetParentId(PurchaseOrderItem child, Guid? parentId)
    {
        var property = typeof(PurchaseOrderItem).GetProperty("GoodId");
        property?.SetValue(child, parentId ?? Guid.Empty);
    }
}

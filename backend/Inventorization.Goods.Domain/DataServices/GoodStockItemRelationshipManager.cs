using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.Domain.PropertyAccessors;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.Domain.DataServices;

/// <summary>
/// Manages Good → StockItem one-to-many relationships.
/// A good can have multiple stock items across different locations.
/// </summary>
public class GoodStockItemRelationshipManager : OneToManyRelationshipManagerBase<Good, StockItem>
{
    public GoodStockItemRelationshipManager(
        IRepository<Good> parentRepository,
        IRepository<StockItem> childRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<GoodStockItemRelationshipManager> logger)
        : base(parentRepository, childRepository, unitOfWork, serviceProvider, logger,
               DataModelRelationships.GoodStockItems, typeof(StockItemGoodIdAccessor))
    {
    }

    protected override void SetParentId(StockItem child, Guid? parentId)
    {
        var property = typeof(StockItem).GetProperty("GoodId");
        property?.SetValue(child, parentId ?? Guid.Empty);
    }
}

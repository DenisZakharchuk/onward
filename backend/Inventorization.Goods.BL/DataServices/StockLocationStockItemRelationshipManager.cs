using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.BL.PropertyAccessors;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.BL.DataServices;

/// <summary>
/// Manages StockLocation → StockItem one-to-many relationships.
/// A stock location can contain multiple stock items.
/// </summary>
public class StockLocationStockItemRelationshipManager : OneToManyRelationshipManagerBase<StockLocation, StockItem>
{
    public StockLocationStockItemRelationshipManager(
        IRepository<StockLocation> parentRepository,
        IRepository<StockItem> childRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<StockLocationStockItemRelationshipManager> logger)
        : base(parentRepository, childRepository, unitOfWork, serviceProvider, logger,
               DataModelRelationships.StockLocationItems, typeof(StockItemLocationIdAccessor))
    {
    }

    protected override void SetParentId(StockItem child, Guid? parentId)
    {
        var property = typeof(StockItem).GetProperty("StockLocationId");
        property?.SetValue(child, parentId ?? Guid.Empty);
    }
}

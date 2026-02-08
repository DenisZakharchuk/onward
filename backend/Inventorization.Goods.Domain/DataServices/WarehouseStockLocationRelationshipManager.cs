using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.Domain.PropertyAccessors;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.Domain.DataServices;

/// <summary>
/// Manages Warehouse → StockLocation one-to-many relationships.
/// A warehouse can contain multiple stock locations.
/// </summary>
public class WarehouseStockLocationRelationshipManager : OneToManyRelationshipManagerBase<Warehouse, StockLocation>
{
    public WarehouseStockLocationRelationshipManager(
        IRepository<Warehouse> parentRepository,
        IRepository<StockLocation> childRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<WarehouseStockLocationRelationshipManager> logger)
        : base(parentRepository, childRepository, unitOfWork, serviceProvider, logger,
               DataModelRelationships.WarehouseStockLocations, typeof(StockLocationWarehouseIdAccessor))
    {
    }

    protected override void SetParentId(StockLocation child, Guid? parentId)
    {
        var property = typeof(StockLocation).GetProperty("WarehouseId");
        property?.SetValue(child, parentId ?? Guid.Empty);
    }
}

using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.PropertyAccessors;

/// <summary>
/// Property accessor for StockLocation.WarehouseId
/// Used for managing StockLocation-to-Warehouse relationship
/// </summary>
public class StockLocationWarehouseIdAccessor : PropertyAccessor<StockLocation, Guid>
{
    public StockLocationWarehouseIdAccessor() 
        : base(stockLocation => stockLocation.WarehouseId)
    {
    }
}

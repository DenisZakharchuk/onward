using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.PropertyAccessors;

/// <summary>
/// Property accessor for StockItem.StockLocationId
/// Used for managing StockItem-to-StockLocation relationship
/// </summary>
public class StockItemLocationIdAccessor : PropertyAccessor<StockItem, Guid>
{
    public StockItemLocationIdAccessor() 
        : base(stockItem => stockItem.StockLocationId)
    {
    }
}

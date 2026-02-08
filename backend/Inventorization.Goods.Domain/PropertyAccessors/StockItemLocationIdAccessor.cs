using Inventorization.Goods.Domain.Entities;

namespace Inventorization.Goods.Domain.PropertyAccessors;

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

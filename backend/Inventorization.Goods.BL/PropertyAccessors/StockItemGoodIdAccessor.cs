using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.PropertyAccessors;

/// <summary>
/// Property accessor for StockItem.GoodId
/// Used for managing StockItem-to-Good relationship
/// </summary>
public class StockItemGoodIdAccessor : PropertyAccessor<StockItem, Guid>
{
    public StockItemGoodIdAccessor() 
        : base(stockItem => stockItem.GoodId)
    {
    }
}

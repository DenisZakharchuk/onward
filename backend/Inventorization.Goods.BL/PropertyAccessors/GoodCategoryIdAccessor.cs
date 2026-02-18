using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.PropertyAccessors;

/// <summary>
/// Property accessor for Good.CategoryId
/// Used for managing Good-to-Category relationship
/// </summary>
public class GoodCategoryIdAccessor : PropertyAccessor<Good, Guid?>
{
    public GoodCategoryIdAccessor() 
        : base(good => good.CategoryId)
    {
    }
}

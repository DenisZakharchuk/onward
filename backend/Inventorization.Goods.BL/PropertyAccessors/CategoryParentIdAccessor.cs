using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.PropertyAccessors;

/// <summary>
/// Property accessor for Category.ParentCategoryId
/// Used for managing Category self-referencing hierarchical relationship
/// </summary>
public class CategoryParentIdAccessor : PropertyAccessor<Category, Guid?>
{
    public CategoryParentIdAccessor() 
        : base(category => category.ParentCategoryId)
    {
    }
}

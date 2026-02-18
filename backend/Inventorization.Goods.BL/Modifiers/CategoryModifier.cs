using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.Category;

namespace Inventorization.Goods.BL.Modifiers;

/// <summary>
/// Updates Category entities from UpdateCategoryDTO
/// </summary>
public class CategoryModifier : IEntityModifier<Category, UpdateCategoryDTO>
{
    public void Modify(Category entity, UpdateCategoryDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        entity.Update(name: dto.Name, description: dto.Description);
        
        if (dto.ParentCategoryId.HasValue)
        {
            entity.SetParentCategory(dto.ParentCategoryId.Value);
        }
        else if (entity.ParentCategoryId.HasValue)
        {
            // Clear parent if null was provided
            entity.SetParentCategory(Guid.Empty);
        }
    }
}

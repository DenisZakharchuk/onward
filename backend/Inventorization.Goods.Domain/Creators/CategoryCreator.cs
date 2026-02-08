using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Category;

namespace Inventorization.Goods.Domain.Creators;

/// <summary>
/// Creates Category entities from CreateCategoryDTO
/// </summary>
public class CategoryCreator : IEntityCreator<Category, CreateCategoryDTO>
{
    public Category Create(CreateCategoryDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var category = new Category(name: dto.Name);
        
        // Set optional properties via Update method to avoid reflection
        if (!string.IsNullOrWhiteSpace(dto.Description) || dto.ParentCategoryId.HasValue)
        {
            category.Update(dto.Name, dto.Description);
            
            if (dto.ParentCategoryId.HasValue)
            {
                category.SetParentCategory(dto.ParentCategoryId.Value);
            }
        }
        
        return category;
    }
}

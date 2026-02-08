using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Category;

namespace Inventorization.Goods.Domain.Mappers;

/// <summary>
/// Maps Category entities to CategoryDetailsDTO
/// </summary>
public class CategoryMapper : IMapper<Category, CategoryDetailsDTO>
{
    public CategoryDetailsDTO Map(Category entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        return new CategoryDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ParentCategoryId = entity.ParentCategoryId == Guid.Empty ? null : entity.ParentCategoryId,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ParentCategory = entity.ParentCategory != null ? new CategoryDetailsDTO
            {
                Id = entity.ParentCategory.Id,
                Name = entity.ParentCategory.Name,
                Description = entity.ParentCategory.Description,
                ParentCategoryId = entity.ParentCategory.ParentCategoryId == Guid.Empty ? null : entity.ParentCategory.ParentCategoryId,
                IsActive = entity.ParentCategory.IsActive,
                CreatedAt = entity.ParentCategory.CreatedAt,
                UpdatedAt = entity.ParentCategory.UpdatedAt
            } : null
        };
    }
    
    public Expression<Func<Category, CategoryDetailsDTO>> GetProjection()
    {
        return entity => new CategoryDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ParentCategoryId = entity.ParentCategoryId == Guid.Empty ? null : entity.ParentCategoryId,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ParentCategory = entity.ParentCategory != null ? new CategoryDetailsDTO
            {
                Id = entity.ParentCategory.Id,
                Name = entity.ParentCategory.Name,
                Description = entity.ParentCategory.Description,
                ParentCategoryId = entity.ParentCategory.ParentCategoryId == Guid.Empty ? null : entity.ParentCategory.ParentCategoryId,
                IsActive = entity.ParentCategory.IsActive,
                CreatedAt = entity.ParentCategory.CreatedAt,
                UpdatedAt = entity.ParentCategory.UpdatedAt
            } : null
        };
    }
}

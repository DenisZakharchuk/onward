using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Category;

namespace InventorySystem.Business.Creators;

/// <summary>
/// Creates Category entities from CreateCategoryDTO
/// </summary>
public class CategoryCreator : IEntityCreator<Category, CreateCategoryDTO>
{
    public Category Create(CreateCategoryDTO dto)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };
    }
}

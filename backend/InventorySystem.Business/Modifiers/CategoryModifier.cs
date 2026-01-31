using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Category;

namespace InventorySystem.Business.Modifiers;

/// <summary>
/// Updates Category entities from UpdateCategoryDTO
/// </summary>
public class CategoryModifier : IEntityModifier<Category, UpdateCategoryDTO>
{
    public void Modify(Category entity, UpdateCategoryDTO dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}

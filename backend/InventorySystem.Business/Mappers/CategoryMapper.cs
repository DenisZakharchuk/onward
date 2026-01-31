using System.Linq.Expressions;
using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Category;

namespace InventorySystem.Business.Mappers;

/// <summary>
/// Maps Category entities to CategoryDetailsDTO
/// </summary>
public class CategoryMapper : IMapper<Category, CategoryDetailsDTO>
{
    public CategoryDetailsDTO Map(Category entity)
    {
        return new CategoryDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public Expression<Func<Category, CategoryDetailsDTO>> GetProjection()
    {
        return c => new CategoryDetailsDTO
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
}

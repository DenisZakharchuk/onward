using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Category;

namespace InventorySystem.Business.Abstractions.Services;

/// <summary>
/// Category data service interface
/// </summary>
public interface ICategoryService : IDataService<Category, CreateCategoryDTO, UpdateCategoryDTO, Inventorization.Base.DTOs.DeleteDTO, InitCategoryDTO, CategoryDetailsDTO, CategorySearchDTO>
{
}

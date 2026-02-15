using InventorySystem.Business.Abstractions.Services;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Category;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Base.DTOs;

namespace InventorySystem.API.Controllers.Data;

/// <summary>
/// Categories CRUD controller using generic DataController base.
/// All CRUD logic is inherited from DataController.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : DataController<Category, CreateCategoryDTO, UpdateCategoryDTO, DeleteDTO, InitCategoryDTO, CategoryDetailsDTO, CategorySearchDTO, ICategoryService>
{
    public CategoriesController(
        ICategoryService categoryService,
        ILogger<ServiceController> logger)
        : base(categoryService, logger)
    {
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Base.Abstractions;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.Domain.DataServices;
using Inventorization.Goods.DTO.DTO.Category;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for managing Categories
/// Inherits all CRUD operations from DataController base class
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class CategoriesController : DataController<Category, CreateCategoryDTO, UpdateCategoryDTO, DeleteCategoryDTO, InitCategoryDTO, CategoryDetailsDTO, CategorySearchDTO, ICategoryDataService>
{
    public CategoriesController(
        ICategoryDataService service,
        ILogger<InventorySystem.API.Base.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

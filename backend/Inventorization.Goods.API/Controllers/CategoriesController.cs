using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onward.Base.API.Controllers;
using Onward.Base.Abstractions;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.BL.DataServices;
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
        ILogger<Onward.Base.API.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

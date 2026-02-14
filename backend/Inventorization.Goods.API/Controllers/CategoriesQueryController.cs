using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Base.Abstractions;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.ADTs;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for executing flexible queries on Categories
/// Inherits all query logic from BaseQueryController
/// </summary>
[ApiController]
[Route("api/categories/query")]
[AllowAnonymous]
public class CategoriesQueryController : BaseQueryController<Category, CategoryProjection>
{
    public CategoriesQueryController(
        ISearchService<Category, CategoryProjection> searchService,
        ILogger<CategoriesQueryController> logger)
        : base(searchService, logger)
    {
    }
}

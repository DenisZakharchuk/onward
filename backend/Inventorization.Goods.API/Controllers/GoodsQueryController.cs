using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Base.Abstractions;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.ADTs;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for executing flexible queries on Goods
/// Inherits all query logic from BaseQueryController
/// </summary>
[ApiController]
[Route("api/goods/query")]
[AllowAnonymous]
public class GoodsQueryController : BaseQueryController<Good, GoodProjection>
{
    public GoodsQueryController(
        ISearchService<Good, GoodProjection> searchService,
        ILogger<GoodsQueryController> logger)
        : base(searchService, logger)
    {
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Base.Abstractions;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.BL.DataServices;
using Inventorization.Goods.DTO.DTO.Good;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for managing Goods (products/items)
/// Inherits all CRUD operations from DataController base class
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class GoodsController : DataController<Good, CreateGoodDTO, UpdateGoodDTO, DeleteGoodDTO, InitGoodDTO, GoodDetailsDTO, GoodSearchDTO, IGoodDataService>
{
    public GoodsController(
        IGoodDataService service,
        ILogger<InventorySystem.API.Base.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

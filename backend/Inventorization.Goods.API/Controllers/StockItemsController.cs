using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onward.Base.API.Controllers;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.BL.DataServices;
using Inventorization.Goods.DTO.DTO.StockItem;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for managing Stock Items
/// Inherits all CRUD operations from DataController base class
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockItemsController : DataController<StockItem, CreateStockItemDTO, UpdateStockItemDTO, DeleteStockItemDTO, InitStockItemDTO, StockItemDetailsDTO, StockItemSearchDTO, IStockItemDataService>
{
    public StockItemsController(IStockItemDataService service, ILogger<Onward.Base.API.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

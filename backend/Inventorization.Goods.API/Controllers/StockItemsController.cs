using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.Domain.DataServices;
using Inventorization.Goods.DTO.DTO.StockItem;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for managing Stock Items
/// Inherits all CRUD operations from DataController base class
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockItemsController : DataController<StockItem, CreateStockItemDTO, UpdateStockItemDTO, DeleteStockItemDTO, StockItemDetailsDTO, StockItemSearchDTO, IStockItemDataService>
{
    public StockItemsController(IStockItemDataService service, ILogger<InventorySystem.API.Base.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

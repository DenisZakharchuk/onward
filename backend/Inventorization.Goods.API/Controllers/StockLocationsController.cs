using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.Domain.DataServices;
using Inventorization.Goods.DTO.DTO.StockLocation;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for managing Stock Locations
/// Inherits all CRUD operations from DataController base class
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockLocationsController : DataController<StockLocation, CreateStockLocationDTO, UpdateStockLocationDTO, DeleteStockLocationDTO, StockLocationDetailsDTO, StockLocationSearchDTO, IStockLocationDataService>
{
    public StockLocationsController(IStockLocationDataService service, ILogger<InventorySystem.API.Base.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

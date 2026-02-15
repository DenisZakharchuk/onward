using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.Domain.DataServices;
using Inventorization.Goods.DTO.DTO.Warehouse;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for managing Warehouses
/// Inherits all CRUD operations from DataController base class
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehousesController : DataController<Warehouse, CreateWarehouseDTO, UpdateWarehouseDTO, DeleteWarehouseDTO, InitWarehouseDTO, WarehouseDetailsDTO, WarehouseSearchDTO, IWarehouseDataService>
{
    public WarehousesController(IWarehouseDataService service, ILogger<InventorySystem.API.Base.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

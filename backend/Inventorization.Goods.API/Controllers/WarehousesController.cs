using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onward.Base.API.Controllers;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.BL.DataServices;
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
    public WarehousesController(IWarehouseDataService service, ILogger<Onward.Base.API.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

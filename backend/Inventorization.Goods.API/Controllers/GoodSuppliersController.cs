using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.BL.DataServices;
using Inventorization.Goods.DTO.DTO.GoodSupplier;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for managing Good-Supplier relationships (Tier 3 - Full CRUD)
/// Allows direct management of supplier pricing and lead time metadata
/// Inherits all CRUD operations from DataController base class
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoodSuppliersController : DataController<GoodSupplier, CreateGoodSupplierDTO, UpdateGoodSupplierDTO, DeleteGoodSupplierDTO, InitGoodSupplierDTO, GoodSupplierDetailsDTO, GoodSupplierSearchDTO, IGoodSupplierDataService>
{
    public GoodSuppliersController(IGoodSupplierDataService service, ILogger<InventorySystem.API.Base.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

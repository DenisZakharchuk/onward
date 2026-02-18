using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.BL.DataServices;
using Inventorization.Goods.DTO.DTO.Supplier;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for managing Suppliers
/// Inherits all CRUD operations from DataController base class
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : DataController<Supplier, CreateSupplierDTO, UpdateSupplierDTO, DeleteSupplierDTO, InitSupplierDTO, SupplierDetailsDTO, SupplierSearchDTO, ISupplierDataService>
{
    public SuppliersController(ISupplierDataService service, ILogger<InventorySystem.API.Base.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

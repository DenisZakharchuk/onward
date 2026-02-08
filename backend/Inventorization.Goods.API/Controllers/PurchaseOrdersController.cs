using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.Domain.DataServices;
using Inventorization.Goods.DTO.DTO.PurchaseOrder;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for managing Purchase Orders
/// Inherits all CRUD operations from DataController base class
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseOrdersController : DataController<PurchaseOrder, CreatePurchaseOrderDTO, UpdatePurchaseOrderDTO, DeletePurchaseOrderDTO, PurchaseOrderDetailsDTO, PurchaseOrderSearchDTO, IPurchaseOrderDataService>
{
    public PurchaseOrdersController(IPurchaseOrderDataService service, ILogger<InventorySystem.API.Base.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

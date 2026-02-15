using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.Domain.DataServices;
using Inventorization.Goods.DTO.DTO.PurchaseOrderItem;

namespace Inventorization.Goods.API.Controllers;

/// <summary>
/// Controller for managing Purchase Order Items
/// Inherits all CRUD operations from DataController base class
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseOrderItemsController : DataController<PurchaseOrderItem, CreatePurchaseOrderItemDTO, UpdatePurchaseOrderItemDTO, DeletePurchaseOrderItemDTO, InitPurchaseOrderItemDTO, PurchaseOrderItemDetailsDTO, PurchaseOrderItemSearchDTO, IPurchaseOrderItemDataService>
{
    public PurchaseOrderItemsController(IPurchaseOrderItemDataService service, ILogger<InventorySystem.API.Base.Controllers.ServiceController> logger) 
        : base(service, logger)
    {
    }
}

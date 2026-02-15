using InventorySystem.Business.Abstractions.Services;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.StockMovement;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Base.DTOs;

namespace InventorySystem.API.Controllers.Data;

/// <summary>
/// Stock Movements CRUD controller using generic DataController base.
/// All CRUD logic is inherited from DataController.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StockController : DataController<StockMovement, CreateStockMovementDTO, UpdateDTO, DeleteDTO, InitStockMovementDTO, StockMovementDetailsDTO, StockMovementSearchDTO, IStockMovementService>
{
    public StockController(
        IStockMovementService stocking,
        ILogger<ServiceController> logger)
        : base(stocking, logger)
    {
    }
}

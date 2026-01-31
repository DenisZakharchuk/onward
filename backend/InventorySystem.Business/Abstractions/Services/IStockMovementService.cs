using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.StockMovement;

namespace InventorySystem.Business.Abstractions.Services;

/// <summary>
/// Stock Movement data service interface
/// </summary>
public interface IStockMovementService : IDataService<
    StockMovement,
    CreateStockMovementDTO,
    Inventorization.Base.DTOs.UpdateDTO,
    Inventorization.Base.DTOs.DeleteDTO,
    StockMovementDetailsDTO,
    StockMovementSearchDTO>
{
}

using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.StockMovement;

namespace InventorySystem.Business.Creators;

/// <summary>
/// Creates StockMovement entities from CreateStockMovementDTO
/// </summary>
public class StockMovementCreator : IEntityCreator<StockMovement, CreateStockMovementDTO>
{
    public StockMovement Create(CreateStockMovementDTO dto)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            Type = (InventorySystem.DataAccess.Models.MovementType)dto.Type,
            Quantity = dto.Quantity,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
    }
}

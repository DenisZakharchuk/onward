using Inventorization.Base.DTOs;

namespace InventorySystem.DTOs.DTO.StockMovement;

/// <summary>
/// StockMovement Search DTO
/// </summary>
public class StockMovementSearchDTO : SearchDTO
{
    public Guid? ProductId { get; set; }
    public MovementType? Type { get; set; }
}

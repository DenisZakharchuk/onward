using Inventorization.Base.DTOs;

namespace InventorySystem.DTOs.DTO.StockMovement;

/// <summary>
/// Create StockMovement DTO
/// </summary>
public class CreateStockMovementDTO : CreateDTO
{
    public Guid ProductId { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

using Inventorization.Base.DTOs;

namespace InventorySystem.DTOs.DTO.StockMovement;

/// <summary>
/// StockMovement Details DTO (returned from Get operations)
/// </summary>
public class StockMovementDetailsDTO : DetailsDTO
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

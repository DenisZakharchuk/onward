namespace InventorySystem.DTOs;

public class CreateStockMovementDto
{
    public Guid ProductId { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

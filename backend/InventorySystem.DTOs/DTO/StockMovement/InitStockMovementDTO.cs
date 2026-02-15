namespace InventorySystem.DTOs.DTO.StockMovement;

public record InitStockMovementDTO(Guid Id, Guid ProductId, MovementType Type, int Quantity) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitStockMovementDTO() : this(Guid.Empty, Guid.Empty, default, 0)
    {
    }
}

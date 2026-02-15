namespace InventorySystem.DTOs.DTO.Product;

public record InitProductDTO(
    Guid Id,
    string Name,
    decimal Price,
    Guid CategoryId,
    int InitialStock,
    int MinimumStock
) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitProductDTO() : this(Guid.Empty, default!, 0, Guid.Empty, 0, 0)
    {
    }
}

namespace InventorySystem.DTOs.DTO.Category;

public record InitCategoryDTO(Guid Id, string Name) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitCategoryDTO() : this(Guid.Empty, default!)
    {
    }
}

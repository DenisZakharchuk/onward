namespace Inventorization.Goods.DTO.DTO.Category;

public record InitCategoryDTO(Guid Id, string Name) : Onward.Base.DTOs.InitDTO(Id)
{
    public InitCategoryDTO() : this(Guid.Empty, default!)
    {
    }
}

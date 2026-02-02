namespace Inventorization.Goods.DTO.DTO.Good;

/// <summary>
/// DTO for creating a new Good entity
/// </summary>
public class CreateGoodDTO : CreateDTO
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = null!;
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
    public string Sku { get; set; } = null!;
    
    [Range(0, double.MaxValue, ErrorMessage = "Unit price must be non-negative")]
    public decimal UnitPrice { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Quantity in stock must be non-negative")]
    public int QuantityInStock { get; set; }
    
    [StringLength(50, ErrorMessage = "Unit of measure cannot exceed 50 characters")]
    public string? UnitOfMeasure { get; set; }
}

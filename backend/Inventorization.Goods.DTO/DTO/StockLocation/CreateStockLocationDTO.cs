namespace Inventorization.Goods.DTO.DTO.StockLocation;

/// <summary>
/// DTO for creating a new StockLocation entity
/// </summary>
public class CreateStockLocationDTO : CreateDTO
{
    [Required(ErrorMessage = "Warehouse ID is required")]
    public Guid WarehouseId { get; set; }
    
    [Required(ErrorMessage = "Code is required")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    public string Code { get; set; } = null!;
    
    [StringLength(50, ErrorMessage = "Aisle cannot exceed 50 characters")]
    public string? Aisle { get; set; }
    
    [StringLength(50, ErrorMessage = "Shelf cannot exceed 50 characters")]
    public string? Shelf { get; set; }
    
    [StringLength(50, ErrorMessage = "Bin cannot exceed 50 characters")]
    public string? Bin { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

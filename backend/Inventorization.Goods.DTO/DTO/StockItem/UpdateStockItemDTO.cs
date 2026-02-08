namespace Inventorization.Goods.DTO.DTO.StockItem;

/// <summary>
/// DTO for updating an existing StockItem entity
/// </summary>
public class UpdateStockItemDTO : UpdateDTO
{
    [Required(ErrorMessage = "Good ID is required")]
    public Guid GoodId { get; set; }
    
    [Required(ErrorMessage = "Stock location ID is required")]
    public Guid StockLocationId { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
    public int Quantity { get; set; }
    
    [StringLength(50, ErrorMessage = "Batch number cannot exceed 50 characters")]
    public string? BatchNumber { get; set; }
    
    [StringLength(100, ErrorMessage = "Serial number cannot exceed 100 characters")]
    public string? SerialNumber { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
}

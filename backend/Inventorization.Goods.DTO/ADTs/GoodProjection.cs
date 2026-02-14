namespace Inventorization.Goods.DTO.ADTs;

/// <summary>
/// Flexible projection DTO for Good search results.
/// Contains only the fields requested in the projection.
/// Uses nested projections for related entities to maintain deep structure.
/// </summary>
public class GoodProjection
{
    // Core fields
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Sku { get; set; }
    
    // Pricing and inventory
    public decimal? UnitPrice { get; set; }
    public int? QuantityInStock { get; set; }
    public string? UnitOfMeasure { get; set; }
    
    // Relationships
    public Guid? CategoryId { get; set; }
    public CategoryProjection? Category { get; set; }
    
    // Status and audit
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

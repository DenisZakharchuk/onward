using Inventorization.Base.Models;

namespace Inventorization.Goods.Domain.Entities;

/// <summary>
/// Represents a specific location within a warehouse (e.g., aisle, shelf, bin).
/// Follows entity immutability pattern with private setters and state mutation methods.
/// </summary>
public class StockLocation : BaseEntity
{
    // Private parameterless constructor for EF Core
    private StockLocation() { }
    
    /// <summary>
    /// Creates a new StockLocation entity with required properties
    /// </summary>
    public StockLocation(Guid warehouseId, string code)
    {
        if (warehouseId == Guid.Empty) 
            throw new ArgumentException("Warehouse ID is required", nameof(warehouseId));
        if (string.IsNullOrWhiteSpace(code)) 
            throw new ArgumentException("Code is required", nameof(code));
        
        Id = Guid.NewGuid();
        WarehouseId = warehouseId;
        Code = code;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid WarehouseId { get; private set; }
    public string Code { get; private set; } = null!;
    public string? Aisle { get; private set; }
    public string? Shelf { get; private set; }
    public string? Bin { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public Warehouse Warehouse { get; private set; } = null!;
    public ICollection<StockItem> StockItems { get; } = new List<StockItem>();
    
    /// <summary>
    /// Updates the StockLocation's information
    /// </summary>
    public void Update(string code, string? aisle, string? shelf, string? bin, string? description)
    {
        if (string.IsNullOrWhiteSpace(code)) 
            throw new ArgumentException("Code is required", nameof(code));
        
        Code = code;
        Aisle = aisle;
        Shelf = shelf;
        Bin = bin;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Updates the warehouse assignment
    /// </summary>
    public void UpdateWarehouse(Guid warehouseId)
    {
        if (warehouseId == Guid.Empty) 
            throw new ArgumentException("Warehouse ID is required", nameof(warehouseId));
        
        WarehouseId = warehouseId;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Deactivates the StockLocation (soft delete)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Activates the StockLocation
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

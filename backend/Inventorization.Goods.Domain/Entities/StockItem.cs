using Inventorization.Base.Models;

namespace Inventorization.Goods.Domain.Entities;

/// <summary>
/// Represents specific quantities of a good at a stock location.
/// Tracks inventory levels at the granular location level.
/// Follows entity immutability pattern with private setters and state mutation methods.
/// </summary>
public class StockItem : BaseEntity
{
    // Private parameterless constructor for EF Core
    private StockItem() { }
    
    /// <summary>
    /// Creates a new StockItem entity with required properties
    /// </summary>
    public StockItem(Guid goodId, Guid stockLocationId, int quantity)
    {
        if (goodId == Guid.Empty) 
            throw new ArgumentException("Good ID is required", nameof(goodId));
        if (stockLocationId == Guid.Empty) 
            throw new ArgumentException("Stock location ID is required", nameof(stockLocationId));
        if (quantity < 0) 
            throw new ArgumentException("Quantity must be non-negative", nameof(quantity));
        
        Id = Guid.NewGuid();
        GoodId = goodId;
        StockLocationId = stockLocationId;
        Quantity = quantity;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid GoodId { get; private set; }
    public Guid StockLocationId { get; private set; }
    public int Quantity { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? SerialNumber { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public Good Good { get; private set; } = null!;
    public StockLocation StockLocation { get; private set; } = null!;
    
    /// <summary>
    /// Updates the quantity
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity < 0) 
            throw new ArgumentException("Quantity must be non-negative", nameof(newQuantity));
        
        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Adjusts the quantity by a delta (can be positive or negative)
    /// </summary>
    public void AdjustQuantity(int delta)
    {
        var newQuantity = Quantity + delta;
        if (newQuantity < 0) 
            throw new InvalidOperationException($"Cannot adjust quantity by {delta}. Resulting quantity would be negative.");
        
        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Updates tracking information (batch, serial, expiry)
    /// </summary>
    public void UpdateTrackingInfo(string? batchNumber, string? serialNumber, DateTime? expiryDate)
    {
        BatchNumber = batchNumber;
        SerialNumber = serialNumber;
        ExpiryDate = expiryDate;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Moves the stock item to a different location
    /// </summary>
    public void MoveToLocation(Guid newStockLocationId)
    {
        if (newStockLocationId == Guid.Empty) 
            throw new ArgumentException("Stock location ID is required", nameof(newStockLocationId));
        
        StockLocationId = newStockLocationId;
        UpdatedAt = DateTime.UtcNow;
    }
}

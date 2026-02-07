using Inventorization.Base.Models;

namespace Inventorization.Goods.Domain.Entities;

/// <summary>
/// Represents an item (line) in a purchase order.
/// Follows entity immutability pattern with private setters and state mutation methods.
/// </summary>
public class PurchaseOrderItem : BaseEntity
{
    // Private parameterless constructor for EF Core
    private PurchaseOrderItem() { }
    
    /// <summary>
    /// Creates a new PurchaseOrderItem entity with required properties
    /// </summary>
    public PurchaseOrderItem(Guid purchaseOrderId, Guid goodId, int quantity, decimal unitPrice)
    {
        if (purchaseOrderId == Guid.Empty) 
            throw new ArgumentException("Purchase order ID is required", nameof(purchaseOrderId));
        if (goodId == Guid.Empty) 
            throw new ArgumentException("Good ID is required", nameof(goodId));
        if (quantity <= 0) 
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (unitPrice < 0) 
            throw new ArgumentException("Unit price must be non-negative", nameof(unitPrice));
        
        Id = Guid.NewGuid();
        PurchaseOrderId = purchaseOrderId;
        GoodId = goodId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        ReceivedQuantity = 0;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid PurchaseOrderId { get; private set; }
    public Guid GoodId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int ReceivedQuantity { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Computed property
    public decimal TotalPrice => Quantity * UnitPrice;
    public bool IsFullyReceived => ReceivedQuantity >= Quantity;
    
    // Navigation properties
    public PurchaseOrder PurchaseOrder { get; private set; } = null!;
    public Good Good { get; private set; } = null!;
    
    /// <summary>
    /// Updates the item details
    /// </summary>
    public void Update(int quantity, decimal unitPrice, string? notes)
    {
        if (quantity <= 0) 
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (unitPrice < 0) 
            throw new ArgumentException("Unit price must be non-negative", nameof(unitPrice));
        
        Quantity = quantity;
        UnitPrice = unitPrice;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Records received quantity
    /// </summary>
    public void RecordReceivedQuantity(int receivedQuantity)
    {
        if (receivedQuantity < 0) 
            throw new ArgumentException("Received quantity must be non-negative", nameof(receivedQuantity));
        if (receivedQuantity > Quantity)
            throw new ArgumentException($"Received quantity ({receivedQuantity}) cannot exceed ordered quantity ({Quantity})");
        
        ReceivedQuantity = receivedQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Adds to the received quantity (partial receipts)
    /// </summary>
    public void AddReceivedQuantity(int additionalQuantity)
    {
        if (additionalQuantity < 0) 
            throw new ArgumentException("Additional quantity must be non-negative", nameof(additionalQuantity));
        
        var newReceivedQuantity = ReceivedQuantity + additionalQuantity;
        if (newReceivedQuantity > Quantity)
            throw new InvalidOperationException($"Total received quantity ({newReceivedQuantity}) would exceed ordered quantity ({Quantity})");
        
        ReceivedQuantity = newReceivedQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
}

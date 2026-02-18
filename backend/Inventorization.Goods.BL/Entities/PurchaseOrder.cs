using Inventorization.Base.Models;
using Inventorization.Goods.Common.Enums;

namespace Inventorization.Goods.BL.Entities;

/// <summary>
/// Represents a purchase order for goods from a supplier.
/// Follows entity immutability pattern with private setters and state mutation methods.
/// </summary>
public class PurchaseOrder : BaseEntity
{
    // Private parameterless constructor for EF Core
    private PurchaseOrder() { }
    
    /// <summary>
    /// Creates a new PurchaseOrder entity with required properties
    /// </summary>
    public PurchaseOrder(string orderNumber, Guid supplierId, DateTime orderDate)
    {
        if (string.IsNullOrWhiteSpace(orderNumber)) 
            throw new ArgumentException("Order number is required", nameof(orderNumber));
        if (supplierId == Guid.Empty) 
            throw new ArgumentException("Supplier ID is required", nameof(supplierId));
        
        Id = Guid.NewGuid();
        OrderNumber = orderNumber;
        SupplierId = supplierId;
        OrderDate = orderDate;
        Status = PurchaseOrderStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }
    
    public string OrderNumber { get; private set; } = null!;
    public Guid SupplierId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public DateTime? ActualDeliveryDate { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public Supplier Supplier { get; private set; } = null!;
    public ICollection<PurchaseOrderItem> Items { get; } = new List<PurchaseOrderItem>();
    
    /// <summary>
    /// Updates the purchase order information
    /// </summary>
    public void Update(string orderNumber, DateTime orderDate, DateTime? expectedDeliveryDate, string? notes)
    {
        if (string.IsNullOrWhiteSpace(orderNumber)) 
            throw new ArgumentException("Order number is required", nameof(orderNumber));
        
        OrderNumber = orderNumber;
        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Submits the purchase order (Draft → Submitted)
    /// </summary>
    public void Submit()
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException($"Cannot submit purchase order in {Status} status");
        
        if (!Items.Any())
            throw new InvalidOperationException("Cannot submit purchase order without items");
        
        Status = PurchaseOrderStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Approves the purchase order (Submitted → Approved)
    /// </summary>
    public void Approve()
    {
        if (Status != PurchaseOrderStatus.Submitted)
            throw new InvalidOperationException($"Cannot approve purchase order in {Status} status");
        
        Status = PurchaseOrderStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marks the purchase order as received (Approved → Received)
    /// </summary>
    public void MarkAsReceived(DateTime actualDeliveryDate)
    {
        if (Status != PurchaseOrderStatus.Approved)
            throw new InvalidOperationException($"Cannot mark purchase order as received in {Status} status");
        
        Status = PurchaseOrderStatus.Received;
        ActualDeliveryDate = actualDeliveryDate;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Cancels the purchase order
    /// </summary>
    public void Cancel()
    {
        if (Status == PurchaseOrderStatus.Received)
            throw new InvalidOperationException("Cannot cancel a received purchase order");
        
        Status = PurchaseOrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Adds an item to the purchase order
    /// </summary>
    public void AddItem(Guid goodId, int quantity, decimal unitPrice)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException($"Cannot add items to purchase order in {Status} status");
        
        Items.Add(new PurchaseOrderItem(Id, goodId, quantity, unitPrice));
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Removes an item from the purchase order
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException($"Cannot remove items from purchase order in {Status} status");
        
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException($"Item {itemId} not found in purchase order");
        
        Items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Calculates the total amount of the purchase order
    /// </summary>
    public decimal CalculateTotal()
    {
        return Items.Sum(i => i.TotalPrice);
    }
}

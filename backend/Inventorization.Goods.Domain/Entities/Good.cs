using Inventorization.Base.Models;

namespace Inventorization.Goods.Domain.Entities;

/// <summary>
/// Represents a Good (product/item) in the inventory system.
/// Follows entity immutability pattern with private setters and state mutation methods.
/// </summary>
public class Good : BaseEntity
{
    // Private parameterless constructor for EF Core
    private Good() { }
    
    /// <summary>
    /// Creates a new Good entity with required properties
    /// </summary>
    public Good(string name, string sku, decimal unitPrice, int quantityInStock)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(sku)) 
            throw new ArgumentException("SKU is required", nameof(sku));
        if (unitPrice < 0) 
            throw new ArgumentException("Unit price must be non-negative", nameof(unitPrice));
        if (quantityInStock < 0) 
            throw new ArgumentException("Quantity in stock must be non-negative", nameof(quantityInStock));
        
        Id = Guid.NewGuid();
        Name = name;
        Sku = sku;
        UnitPrice = unitPrice;
        QuantityInStock = quantityInStock;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Sku { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int QuantityInStock { get; private set; }
    public string? UnitOfMeasure { get; private set; }
    public Guid? CategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public Category? Category { get; private set; }
    public ICollection<GoodSupplier> GoodSuppliers { get; } = new List<GoodSupplier>();
    public ICollection<StockItem> StockItems { get; } = new List<StockItem>();
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; } = new List<PurchaseOrderItem>();
    
    /// <summary>
    /// Updates the Good's basic information
    /// </summary>
    public void Update(string name, string? description, string sku, decimal unitPrice, 
        int quantityInStock, string? unitOfMeasure)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(sku)) 
            throw new ArgumentException("SKU is required", nameof(sku));
        if (unitPrice < 0) 
            throw new ArgumentException("Unit price must be non-negative", nameof(unitPrice));
        if (quantityInStock < 0) 
            throw new ArgumentException("Quantity in stock must be non-negative", nameof(quantityInStock));
        
        Name = name;
        Description = description;
        Sku = sku;
        UnitPrice = unitPrice;
        QuantityInStock = quantityInStock;
        UnitOfMeasure = unitOfMeasure;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Sets the category for this good
    /// </summary>
    public void SetCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Updates the quantity in stock
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity < 0) 
            throw new ArgumentException("Quantity must be non-negative", nameof(newQuantity));
        
        QuantityInStock = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Updates the unit price
    /// </summary>
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0) 
            throw new ArgumentException("Price must be non-negative", nameof(newPrice));
        
        UnitPrice = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Deactivates the Good (soft delete)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Activates the Good
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

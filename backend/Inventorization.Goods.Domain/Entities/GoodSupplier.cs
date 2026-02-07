using Inventorization.Base.Models;

namespace Inventorization.Goods.Domain.Entities;

/// <summary>
/// Junction entity representing the many-to-many relationship between Goods and Suppliers.
/// Contains additional metadata: supplier price and lead time.
/// This is a Tier 3 relationship (full CRUD with metadata).
/// </summary>
public class GoodSupplier : JunctionEntityBase
{
    // Private parameterless constructor for EF Core
    private GoodSupplier() : base(Guid.Empty, Guid.Empty) { }
    
    /// <summary>
    /// Creates a new GoodSupplier relationship with required properties
    /// </summary>
    public GoodSupplier(Guid goodId, Guid supplierId, decimal supplierPrice, int leadTimeDays)
        : base(goodId, supplierId)
    {
        if (supplierPrice < 0)
            throw new ArgumentException("Supplier price must be non-negative", nameof(supplierPrice));
        if (leadTimeDays < 0)
            throw new ArgumentException("Lead time days must be non-negative", nameof(leadTimeDays));
        
        SupplierPrice = supplierPrice;
        LeadTimeDays = leadTimeDays;
        IsPreferred = false;
        CreatedAt = DateTime.UtcNow;
    }
    
    // Property aliases for semantic clarity
    public Guid GoodId => EntityId;
    public Guid SupplierId => RelatedEntityId;
    
    // Additional metadata
    public decimal SupplierPrice { get; private set; }
    public int LeadTimeDays { get; private set; }
    public bool IsPreferred { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public Good Good { get; } = null!;
    public Supplier Supplier { get; } = null!;
    
    /// <summary>
    /// Updates the supplier pricing and lead time
    /// </summary>
    public void UpdatePricing(decimal supplierPrice, int leadTimeDays)
    {
        if (supplierPrice < 0)
            throw new ArgumentException("Supplier price must be non-negative", nameof(supplierPrice));
        if (leadTimeDays < 0)
            throw new ArgumentException("Lead time days must be non-negative", nameof(leadTimeDays));
        
        SupplierPrice = supplierPrice;
        LeadTimeDays = leadTimeDays;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marks this supplier as preferred for this good
    /// </summary>
    public void SetAsPreferred()
    {
        IsPreferred = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Removes preferred status
    /// </summary>
    public void RemovePreferredStatus()
    {
        IsPreferred = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

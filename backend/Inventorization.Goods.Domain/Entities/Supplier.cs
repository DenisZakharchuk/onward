using Inventorization.Base.Models;

namespace Inventorization.Goods.Domain.Entities;

/// <summary>
/// Represents a supplier that provides goods to the inventory system.
/// Follows entity immutability pattern with private setters and state mutation methods.
/// </summary>
public class Supplier : BaseEntity
{
    // Private parameterless constructor for EF Core
    private Supplier() { }
    
    /// <summary>
    /// Creates a new Supplier entity with required properties
    /// </summary>
    public Supplier(string name, string contactEmail)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(contactEmail)) 
            throw new ArgumentException("Contact email is required", nameof(contactEmail));
        
        Id = Guid.NewGuid();
        Name = name;
        ContactEmail = contactEmail;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string ContactEmail { get; private set; } = null!;
    public string? ContactPhone { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public ICollection<GoodSupplier> GoodSuppliers { get; } = new List<GoodSupplier>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; } = new List<PurchaseOrder>();
    
    /// <summary>
    /// Updates the Supplier's information
    /// </summary>
    public void Update(string name, string? description, string contactEmail, 
        string? contactPhone, string? address, string? city, string? country, string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(contactEmail)) 
            throw new ArgumentException("Contact email is required", nameof(contactEmail));
        
        Name = name;
        Description = description;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        Address = address;
        City = city;
        Country = country;
        PostalCode = postalCode;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Assigns a good to this supplier
    /// </summary>
    public void AssignGood(Guid goodId, decimal supplierPrice, int leadTimeDays)
    {
        if (goodId == Guid.Empty)
            throw new ArgumentException("Good ID is required", nameof(goodId));
        
        if (GoodSuppliers.Any(gs => gs.GoodId == goodId))
            throw new InvalidOperationException($"Good {goodId} is already assigned to this supplier");
        
        GoodSuppliers.Add(new GoodSupplier(goodId, Id, supplierPrice, leadTimeDays));
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Removes a good from this supplier
    /// </summary>
    public void RemoveGood(Guid goodId)
    {
        var goodSupplier = GoodSuppliers.FirstOrDefault(gs => gs.GoodId == goodId);
        if (goodSupplier == null)
            throw new InvalidOperationException($"Good {goodId} is not assigned to this supplier");
        
        GoodSuppliers.Remove(goodSupplier);
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Deactivates the Supplier (soft delete)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Activates the Supplier
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

using Inventorization.Base.Models;

namespace Inventorization.Goods.Domain.Entities;

/// <summary>
/// Represents a physical warehouse location that contains stock locations.
/// Follows entity immutability pattern with private setters and state mutation methods.
/// </summary>
public class Warehouse : BaseEntity
{
    /// <summary>
    /// Metadata for this entity - single source of truth for structure and validation
    /// </summary>
    private static readonly IDataModelMetadata<Warehouse> Metadata = DataModelMetadata.Warehouse;
    
    // Private parameterless constructor for EF Core
    private Warehouse() { }
    
    /// <summary>
    /// Creates a new Warehouse entity with required properties
    /// </summary>
    public Warehouse(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) 
            throw new ArgumentException("Code is required", nameof(code));
        
        Id = Guid.NewGuid();
        Name = name;
        Code = code;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }
    public string? ManagerName { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public ICollection<StockLocation> StockLocations { get; } = new List<StockLocation>();
    
    /// <summary>
    /// Updates the Warehouse's information
    /// </summary>
    public void Update(string name, string code, string? description, string? address, 
        string? city, string? country, string? postalCode, string? managerName, string? contactPhone)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) 
            throw new ArgumentException("Code is required", nameof(code));
        
        Name = name;
        Code = code;
        Description = description;
        Address = address;
        City = city;
        Country = country;
        PostalCode = postalCode;
        ManagerName = managerName;
        ContactPhone = contactPhone;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Deactivates the Warehouse (soft delete)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Activates the Warehouse
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

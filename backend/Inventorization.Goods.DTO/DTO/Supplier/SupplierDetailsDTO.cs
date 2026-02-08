namespace Inventorization.Goods.DTO.DTO.Supplier;

/// <summary>
/// DTO for Supplier entity details response
/// </summary>
public class SupplierDetailsDTO : DetailsDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string ContactEmail { get; set; } = null!;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

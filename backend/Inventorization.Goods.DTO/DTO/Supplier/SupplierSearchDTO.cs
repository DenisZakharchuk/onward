namespace Inventorization.Goods.DTO.DTO.Supplier;

/// <summary>
/// DTO for searching Supplier entities
/// </summary>
public class SupplierSearchDTO : SearchDTO
{
    public string? Name { get; set; }
    public string? ContactEmail { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool? IsActive { get; set; }
    public PageDTO Page { get; set; } = new();
}

namespace Inventorization.Goods.DTO.DTO.Warehouse;

/// <summary>
/// DTO for Warehouse entity details response
/// </summary>
public class WarehouseDetailsDTO : DetailsDTO
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? ManagerName { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

namespace Inventorization.Goods.DTO.DTO.Warehouse;

/// <summary>
/// DTO for searching Warehouse entities
/// </summary>
public class WarehouseSearchDTO : SearchDTO
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool? IsActive { get; set; }
    public PageDTO Page { get; set; } = new();
}

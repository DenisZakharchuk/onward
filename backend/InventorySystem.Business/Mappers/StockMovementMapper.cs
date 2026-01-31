using System.Linq.Expressions;
using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.StockMovement;

namespace InventorySystem.Business.Mappers;

/// <summary>
/// Maps StockMovement entities to StockMovementDetailsDTO
/// </summary>
public class StockMovementMapper : IMapper<StockMovement, StockMovementDetailsDTO>
{
    private readonly InventorySystem.DataAccess.Abstractions.IUnitOfWork _unitOfWork;

    public StockMovementMapper(InventorySystem.DataAccess.Abstractions.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public StockMovementDetailsDTO Map(StockMovement entity)
    {
        var product = _unitOfWork.Products.GetByIdAsync(entity.ProductId).Result;
        return new StockMovementDetailsDTO
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ProductName = product?.Name ?? string.Empty,
            Type = (InventorySystem.DTOs.DTO.StockMovement.MovementType)entity.Type,
            Quantity = entity.Quantity,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt
        };
    }

    public Expression<Func<StockMovement, StockMovementDetailsDTO>> GetProjection()
    {
        return sm => new StockMovementDetailsDTO
        {
            Id = sm.Id,
            ProductId = sm.ProductId,
            ProductName = sm.Product != null ? sm.Product.Name : string.Empty,
            Type = (InventorySystem.DTOs.DTO.StockMovement.MovementType)sm.Type,
            Quantity = sm.Quantity,
            Notes = sm.Notes,
            CreatedAt = sm.CreatedAt
        };
    }
}

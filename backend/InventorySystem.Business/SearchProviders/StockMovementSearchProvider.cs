using System.Linq.Expressions;
using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.StockMovement;

namespace InventorySystem.Business.SearchProviders;

/// <summary>
/// Creates search expressions for StockMovement entities
/// </summary>
public class StockMovementSearchProvider : ISearchQueryProvider<StockMovement, StockMovementSearchDTO>
{
    public Expression<Func<StockMovement, bool>> GetSearchExpression(StockMovementSearchDTO searchDto)
    {
        return sm =>
            (!searchDto.ProductId.HasValue || sm.ProductId == searchDto.ProductId) &&
            (!searchDto.Type.HasValue || sm.Type == (InventorySystem.DataAccess.Models.MovementType)searchDto.Type);
    }
}

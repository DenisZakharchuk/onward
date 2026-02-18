using Inventorization.Base.Abstractions;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.ADTs;

namespace Inventorization.Goods.BL.Mappers.Projection;

/// <summary>
/// Concrete interface for Good entity projection mapper.
/// Maps Good entities to GoodProjection DTOs with support for related Category projection.
/// </summary>
public interface IGoodProjectionMapper : IProjectionMapper<Good, GoodProjection>
{
}

using Inventorization.Base.Abstractions;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.ADTs;

namespace Inventorization.Goods.Domain.Mappers.Projection;

/// <summary>
/// Concrete interface for Category entity projection mapper.
/// Maps Category entities to CategoryProjection DTOs with support for recursive ParentCategory navigation.
/// </summary>
public interface ICategoryProjectionMapper : IProjectionMapper<Category, CategoryProjection>
{
}

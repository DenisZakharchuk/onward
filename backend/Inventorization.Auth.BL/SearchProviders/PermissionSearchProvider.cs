using Inventorization.Auth.BL.Entities;
using Inventorization.Auth.DTO.DTO.Permission;
using Inventorization.Base.Abstractions;
using System.Linq.Expressions;

namespace Inventorization.Auth.BL.SearchProviders;

/// <summary>
/// Provides LINQ filter expressions for Permission search queries
/// </summary>
public class PermissionSearchProvider : ISearchQueryProvider<Permission, PermissionSearchDTO>
{
    /// <summary>
    /// Builds search expression based on search criteria
    /// </summary>
    public Expression<Func<Permission, bool>> GetSearchExpression(PermissionSearchDTO search)
    {
        if (search == null) throw new ArgumentNullException(nameof(search));

        return permission =>
            (string.IsNullOrEmpty(search.Name) || permission.Name.Contains(search.Name)) &&
            (string.IsNullOrEmpty(search.Resource) || permission.Resource.Contains(search.Resource)) &&
            (string.IsNullOrEmpty(search.Action) || permission.Action.Contains(search.Action));
    }
}

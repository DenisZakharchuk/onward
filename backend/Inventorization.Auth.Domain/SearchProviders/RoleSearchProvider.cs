using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.DTO.DTO.Role;
using Inventorization.Base.Abstractions;
using System.Linq.Expressions;

namespace Inventorization.Auth.Domain.SearchProviders;

/// <summary>
/// Provides LINQ filter expressions for Role search queries
/// </summary>
public class RoleSearchProvider : ISearchQueryProvider<Role, RoleSearchDTO>
{
    /// <summary>
    /// Builds search expression based on search criteria
    /// </summary>
    public Expression<Func<Role, bool>> GetSearchExpression(RoleSearchDTO search)
    {
        if (search == null) throw new ArgumentNullException(nameof(search));

        return role =>
            (string.IsNullOrEmpty(search.Name) || role.Name.Contains(search.Name)) &&
            (string.IsNullOrEmpty(search.Description) || role.Description == null || role.Description.Contains(search.Description));
    }
}

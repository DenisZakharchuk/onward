using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.DTO.DTO.User;
using Inventorization.Base.Abstractions;
using System.Linq.Expressions;

namespace Inventorization.Auth.Domain.SearchProviders;

/// <summary>
/// Provides LINQ filter expressions for User search queries
/// </summary>
public class UserSearchProvider : ISearchQueryProvider<User, UserSearchDTO>
{
    /// <summary>
    /// Builds search expression based on search criteria
    /// </summary>
    public Expression<Func<User, bool>> GetSearchExpression(UserSearchDTO search)
    {
        if (search == null) throw new ArgumentNullException(nameof(search));

        return user =>
            (string.IsNullOrEmpty(search.Email) || user.Email.Contains(search.Email)) &&
            (string.IsNullOrEmpty(search.FullName) || user.FullName.Contains(search.FullName)) &&
            (user.IsActive == true);  // Only active users by default
    }
}

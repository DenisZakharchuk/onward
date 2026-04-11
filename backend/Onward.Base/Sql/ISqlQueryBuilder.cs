using Onward.Base.ADTs;
using Onward.Base.Models;

namespace Onward.Base.Sql;

/// <summary>
/// A fully parameterized SQL statement ready for execution.
/// </summary>
/// <param name="Sql">The SQL text with dialect-specific parameter placeholders.</param>
/// <param name="Parameters">Ordered list of parameter values matching the placeholders.</param>
public sealed record SqlQuery(string Sql, IReadOnlyList<object?> Parameters);

/// <summary>
/// Builds parameterized SELECT and COUNT SQL statements from a <see cref="SearchQuery"/> ADT,
/// validated against a given <see cref="EntityMetadata"/>.
/// </summary>
public interface ISqlQueryBuilder
{
    /// <summary>
    /// Builds a SELECT statement with WHERE, ORDER BY, and LIMIT/OFFSET clauses.
    /// </summary>
    /// <param name="metadata">Entity metadata used to validate and resolve field names.</param>
    /// <param name="query">The structured search query.</param>
    /// <returns>A parameterized <see cref="SqlQuery"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a referenced field does not exist in <paramref name="metadata"/>,
    /// or when a filter/sort is attempted on an unsupported type (e.g. DateTimeWithOffset).
    /// </exception>
    SqlQuery BuildSelectQuery(EntityMetadata metadata, SearchQuery query);

    /// <summary>
    /// Builds a SELECT COUNT(*) statement with the same WHERE clause as the select query.
    /// Used to retrieve the total row count for pagination metadata.
    /// </summary>
    SqlQuery BuildCountQuery(EntityMetadata metadata, SearchQuery query);
}

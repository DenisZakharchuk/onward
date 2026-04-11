namespace Onward.Base.Sql;

/// <summary>
/// Abstracts SQL syntax differences between database providers.
/// Register a concrete implementation as a singleton per bounded context.
/// </summary>
public interface ISqlDialect
{
    /// <summary>
    /// Wraps an identifier (table name, column name, schema) in the provider-specific quote characters.
    /// PostgreSQL: "name" — SQL Server: [name]
    /// </summary>
    string QuoteIdentifier(string name);

    /// <summary>
    /// Returns a positional parameter placeholder for the given 0-based index.
    /// PostgreSQL: $1, $2, … — SQL Server / SQLite: @p0, @p1, …
    /// </summary>
    string Parameter(int index);

    /// <summary>
    /// Returns a LIMIT / OFFSET (or equivalent) fragment.
    /// PostgreSQL: LIMIT {pageSize} OFFSET {offset}
    /// </summary>
    string LimitOffset(int pageSize, int offset);

    /// <summary>
    /// Returns a SQL CAST expression for the given value expression.
    /// PostgreSQL: CAST({expr} AS {sqlType})
    /// </summary>
    string Cast(string expr, string sqlType);

    /// <summary>
    /// Case-insensitive LIKE operator.
    /// PostgreSQL: ILIKE — SQL Server: LIKE (case-insensitivity controlled by collation)
    /// </summary>
    string LikeOperator { get; }

    /// <summary>
    /// String concatenation operator.
    /// PostgreSQL: || — SQL Server: +
    /// </summary>
    string ConcatOperator { get; }
}

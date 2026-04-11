namespace Onward.Base.Sql;

/// <summary>
/// PostgreSQL-specific SQL dialect implementation.
/// Uses double-quote identifiers, $n positional parameters, and ILIKE for case-insensitive matching.
/// </summary>
public sealed class PostgreSqlDialect : ISqlDialect
{
    /// <summary>Singleton instance — safe to reuse across requests.</summary>
    public static readonly PostgreSqlDialect Instance = new();

    private PostgreSqlDialect() { }

    /// <inheritdoc/>
    public string QuoteIdentifier(string name) => $"\"{name}\"";

    /// <inheritdoc/>
    /// PostgreSQL uses 1-based positional parameters: $1, $2, …
    public string Parameter(int index) => $"${index + 1}";

    /// <inheritdoc/>
    public string LimitOffset(int pageSize, int offset) =>
        $"LIMIT {pageSize} OFFSET {offset}";

    /// <inheritdoc/>
    public string Cast(string expr, string sqlType) => $"CAST({expr} AS {sqlType})";

    /// <inheritdoc/>
    public string LikeOperator => "ILIKE";

    /// <inheritdoc/>
    public string ConcatOperator => "||";
}

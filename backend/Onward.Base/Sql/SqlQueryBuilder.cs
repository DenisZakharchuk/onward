using System.Text;
using Onward.Base.ADTs;
using Onward.Base.Models;

namespace Onward.Base.Sql;

/// <summary>
/// Converts a <see cref="SearchQuery"/> ADT into a parameterized SQL statement,
/// validated against an <see cref="EntityMetadata"/> snapshot.
/// All field references are checked against <c>EntityMetadata.Properties</c> at build time;
/// unknown fields or unsupported types produce a clear <see cref="InvalidOperationException"/>.
/// </summary>
public sealed class SqlQueryBuilder : ISqlQueryBuilder
{
    /// <summary>
    /// Value object property types that span multiple columns and cannot be used
    /// directly in SQL filter or ORDER BY clauses.
    /// </summary>
    private static readonly HashSet<string> BlockedFilterTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "DateTimeWithOffset", "DateTimeWithOffset?"
    };

    private readonly ISqlDialect _dialect;

    public SqlQueryBuilder(ISqlDialect dialect)
    {
        _dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <inheritdoc/>
    public SqlQuery BuildSelectQuery(EntityMetadata metadata, SearchQuery query)
    {
        var parameters = new List<object?>(8);
        var sb = new StringBuilder(256);

        // SELECT
        AppendSelect(sb, metadata, query.Projection);

        // FROM
        sb.Append(" FROM ").Append(TableReference(metadata));

        // WHERE
        if (query.Filter != null)
        {
            sb.Append(" WHERE ");
            AppendFilter(sb, metadata, query.Filter, parameters);
        }

        // ORDER BY
        if (query.Sort?.Fields.Count > 0)
        {
            sb.Append(" ORDER BY ");
            AppendOrderBy(sb, metadata, query.Sort);
        }

        // LIMIT / OFFSET
        var page = query.Pagination;
        var offset = (page.PageNumber - 1) * page.PageSize;
        sb.Append(' ').Append(_dialect.LimitOffset(page.PageSize, offset));

        return new SqlQuery(sb.ToString(), parameters);
    }

    /// <inheritdoc/>
    public SqlQuery BuildCountQuery(EntityMetadata metadata, SearchQuery query)
    {
        var parameters = new List<object?>(8);
        var sb = new StringBuilder(128);

        sb.Append("SELECT COUNT(*) FROM ").Append(TableReference(metadata));

        if (query.Filter != null)
        {
            sb.Append(" WHERE ");
            AppendFilter(sb, metadata, query.Filter, parameters);
        }

        return new SqlQuery(sb.ToString(), parameters);
    }

    // -----------------------------------------------------------------------
    // SELECT clause
    // -----------------------------------------------------------------------

    private void AppendSelect(StringBuilder sb, EntityMetadata metadata, ProjectionRequest? projection)
    {
        sb.Append("SELECT ");

        if (projection == null || projection.IsAllFields || projection.Fields.Count == 0)
        {
            sb.Append('*');
            return;
        }

        bool first = true;
        foreach (var field in projection.Fields)
        {
            if (field.IsRelated) continue; // related fields not supported in SQL projection

            var prop = ResolveProperty(metadata, field.FieldName);

            if (!first) sb.Append(", ");
            first = false;

            sb.Append(_dialect.QuoteIdentifier(ColumnName(prop)));
        }

        if (first) sb.Append('*'); // all fields were related — fall back to *
    }

    // -----------------------------------------------------------------------
    // FROM clause
    // -----------------------------------------------------------------------

    private string TableReference(EntityMetadata metadata)
    {
        var table = _dialect.QuoteIdentifier(metadata.TableName);
        return metadata.SchemaName is null
            ? table
            : $"{_dialect.QuoteIdentifier(metadata.SchemaName)}.{table}";
    }

    // -----------------------------------------------------------------------
    // WHERE clause
    // -----------------------------------------------------------------------

    private void AppendFilter(StringBuilder sb, EntityMetadata metadata, FilterExpression filter, List<object?> parameters)
    {
        switch (filter)
        {
            case LeafFilter leaf:
                AppendCondition(sb, metadata, leaf.Condition, parameters);
                break;

            case AndFilter and:
                if (and.Expressions.Count == 0) { sb.Append("1=1"); return; }
                sb.Append('(');
                for (int i = 0; i < and.Expressions.Count; i++)
                {
                    if (i > 0) sb.Append(" AND ");
                    AppendFilter(sb, metadata, and.Expressions[i], parameters);
                }
                sb.Append(')');
                break;

            case OrFilter or:
                if (or.Expressions.Count == 0) { sb.Append("1=0"); return; }
                sb.Append('(');
                for (int i = 0; i < or.Expressions.Count; i++)
                {
                    if (i > 0) sb.Append(" OR ");
                    AppendFilter(sb, metadata, or.Expressions[i], parameters);
                }
                sb.Append(')');
                break;

            default:
                throw new NotSupportedException($"FilterExpression type '{filter.GetType().Name}' is not supported by SqlQueryBuilder.");
        }
    }

    private void AppendCondition(StringBuilder sb, EntityMetadata metadata, FilterCondition condition, List<object?> parameters)
    {
        var prop = ResolveProperty(metadata, condition.FieldName);
        AssertFilterable(prop);

        var col = _dialect.QuoteIdentifier(ColumnName(prop));

        switch (condition)
        {
            case EqualsCondition eq:
                parameters.Add(eq.Value);
                sb.Append(col).Append(" = ").Append(_dialect.Parameter(parameters.Count - 1));
                break;

            case GreaterThanCondition gt:
                parameters.Add(gt.Value);
                sb.Append(col).Append(" > ").Append(_dialect.Parameter(parameters.Count - 1));
                break;

            case LessThanCondition lt:
                parameters.Add(lt.Value);
                sb.Append(col).Append(" < ").Append(_dialect.Parameter(parameters.Count - 1));
                break;

            case GreaterThanOrEqualCondition gte:
                parameters.Add(gte.Value);
                sb.Append(col).Append(" >= ").Append(_dialect.Parameter(parameters.Count - 1));
                break;

            case LessThanOrEqualCondition lte:
                parameters.Add(lte.Value);
                sb.Append(col).Append(" <= ").Append(_dialect.Parameter(parameters.Count - 1));
                break;

            case ContainsCondition contains:
                parameters.Add($"%{EscapeLikePattern(contains.Value)}%");
                sb.Append(col).Append(' ').Append(_dialect.LikeOperator).Append(' ').Append(_dialect.Parameter(parameters.Count - 1));
                break;

            case StartsWithCondition startsWith:
                parameters.Add($"{EscapeLikePattern(startsWith.Value)}%");
                sb.Append(col).Append(' ').Append(_dialect.LikeOperator).Append(' ').Append(_dialect.Parameter(parameters.Count - 1));
                break;

            case InCondition inCondition:
                if (inCondition.Values.Count == 0) { sb.Append("1=0"); return; }
                sb.Append(col).Append(" IN (");
                for (int i = 0; i < inCondition.Values.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    parameters.Add(inCondition.Values[i]);
                    sb.Append(_dialect.Parameter(parameters.Count - 1));
                }
                sb.Append(')');
                break;

            case IsNullCondition:
                sb.Append(col).Append(" IS NULL");
                break;

            case IsNotNullCondition:
                sb.Append(col).Append(" IS NOT NULL");
                break;

            default:
                throw new NotSupportedException($"FilterCondition type '{condition.GetType().Name}' is not supported by SqlQueryBuilder.");
        }
    }

    // -----------------------------------------------------------------------
    // ORDER BY clause
    // -----------------------------------------------------------------------

    private void AppendOrderBy(StringBuilder sb, EntityMetadata metadata, SortRequest sort)
    {
        for (int i = 0; i < sort.Fields.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            var field = sort.Fields[i];
            var prop = ResolveProperty(metadata, field.FieldName);
            AssertFilterable(prop);
            sb.Append(_dialect.QuoteIdentifier(ColumnName(prop)));
            sb.Append(field.Direction == SortDirection.Descending ? " DESC" : " ASC");
        }
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private PropertyMetadata ResolveProperty(EntityMetadata metadata, string fieldName)
    {
        var prop = Array.Find(metadata.Properties, p => string.Equals(p.PropertyName, fieldName, StringComparison.Ordinal));
        if (prop is null)
            throw new InvalidOperationException(
                $"Field '{fieldName}' does not exist on entity '{metadata.EntityName}'. " +
                $"Available fields: {string.Join(", ", Array.ConvertAll(metadata.Properties, p => p.PropertyName))}");
        return prop;
    }

    private void AssertFilterable(PropertyMetadata prop)
    {
        if (BlockedFilterTypes.Contains(prop.PropertyType))
            throw new InvalidOperationException(
                $"Field '{prop.PropertyName}' has type '{prop.PropertyType}' which spans multiple database columns " +
                $"and cannot be used in a SQL filter or ORDER BY clause directly. " +
                $"Filter on the underlying component columns (e.g. '{prop.PropertyName}_UtcTicks') instead.");
    }

    /// <summary>Returns the SQL column name: explicit override or falls back to property name.</summary>
    private static string ColumnName(PropertyMetadata prop) => prop.ColumnName ?? prop.PropertyName;

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}

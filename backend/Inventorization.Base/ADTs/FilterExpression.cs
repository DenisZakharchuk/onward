using System.Text.Json.Serialization;
using Inventorization.Base.ADTs.Converters;

namespace Inventorization.Base.ADTs;

/// <summary>
/// Base class for composable filter expressions.
/// Supports logical combination of filter conditions using AND/OR.
/// </summary>
[JsonConverter(typeof(FilterExpressionConverter))]
public abstract record FilterExpression;

/// <summary>
/// Leaf node containing a single filter condition
/// </summary>
public sealed record LeafFilter(FilterCondition Condition) : FilterExpression;

/// <summary>
/// Logical AND combination of multiple filter expressions.
/// All expressions must be true for the AND to be true.
/// </summary>
public sealed record AndFilter(IReadOnlyList<FilterExpression> Expressions) : FilterExpression
{
    /// <summary>
    /// Convenience constructor for params-style initialization
    /// </summary>
    public AndFilter(params FilterExpression[] expressions) : this((IReadOnlyList<FilterExpression>)expressions) { }
}

/// <summary>
/// Logical OR combination of multiple filter expressions.
/// At least one expression must be true for the OR to be true.
/// </summary>
public sealed record OrFilter(IReadOnlyList<FilterExpression> Expressions) : FilterExpression
{
    /// <summary>
    /// Convenience constructor for params-style initialization
    /// </summary>
    public OrFilter(params FilterExpression[] expressions) : this((IReadOnlyList<FilterExpression>)expressions) { }
}

namespace Inventorization.Base.ADTs;

/// <summary>
/// Base class for all filter conditions in search queries.
/// Represents a single comparison operation on a field.
/// </summary>
public abstract record FilterCondition(string FieldName);

/// <summary>
/// Equality comparison: field == value
/// </summary>
public sealed record EqualsCondition(string FieldName, object Value) : FilterCondition(FieldName);

/// <summary>
/// Greater than comparison: field > value
/// </summary>
public sealed record GreaterThanCondition(string FieldName, object Value) : FilterCondition(FieldName);

/// <summary>
/// Less than comparison: field &lt; value
/// </summary>
public sealed record LessThanCondition(string FieldName, object Value) : FilterCondition(FieldName);

/// <summary>
/// Greater than or equal comparison: field >= value
/// </summary>
public sealed record GreaterThanOrEqualCondition(string FieldName, object Value) : FilterCondition(FieldName);

/// <summary>
/// Less than or equal comparison: field &lt;= value
/// </summary>
public sealed record LessThanOrEqualCondition(string FieldName, object Value) : FilterCondition(FieldName);

/// <summary>
/// Contains comparison for strings: field.Contains(value)
/// </summary>
public sealed record ContainsCondition(string FieldName, string Value) : FilterCondition(FieldName);

/// <summary>
/// Starts with comparison for strings: field.StartsWith(value)
/// </summary>
public sealed record StartsWithCondition(string FieldName, string Value) : FilterCondition(FieldName);

/// <summary>
/// In comparison: field is in list of values
/// </summary>
public sealed record InCondition(string FieldName, IReadOnlyList<object> Values) : FilterCondition(FieldName);

/// <summary>
/// Null check: field == null
/// </summary>
public sealed record IsNullCondition(string FieldName) : FilterCondition(FieldName);

/// <summary>
/// Not null check: field != null
/// </summary>
public sealed record IsNotNullCondition(string FieldName) : FilterCondition(FieldName);

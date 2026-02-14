namespace Inventorization.Base.ADTs;

/// <summary>
/// Base ADT for projection field transformations supporting unlimited composition.
/// Every ProjectionField can be nested within another ProjectionField.
/// </summary>
public abstract record ProjectionField
{
    /// <summary>
    /// Gets the output type of this projection field (for type safety validation)
    /// </summary>
    public abstract Type GetOutputType();
}

/// <summary>
/// References a field from the source entity
/// </summary>
public sealed record FieldReference : ProjectionField
{
    /// <summary>
    /// Field name or path (e.g., "Name", "Category.Name")
    /// </summary>
    public required string FieldName { get; init; }
    
    /// <summary>
    /// Expected output type (determined during expression building)
    /// </summary>
    public Type? OutputType { get; init; }
    
    public override Type GetOutputType() => OutputType ?? typeof(object);
}

/// <summary>
/// Constant value (string, number, bool, null)
/// </summary>
public sealed record ConstantValue : ProjectionField
{
    /// <summary>
    /// The constant value
    /// </summary>
    public required object? Value { get; init; }
    
    /// <summary>
    /// Type of the constant value
    /// </summary>
    public Type? ValueType { get; init; }
    
    public override Type GetOutputType() => ValueType ?? Value?.GetType() ?? typeof(object);
}

/// <summary>
/// String transformation operations (ToUpper, ToLower, Substring, Concat, etc.)
/// </summary>
public sealed record StringTransform : ProjectionField
{
    /// <summary>
    /// Operation type
    /// </summary>
    public required StringOperation Operation { get; init; }
    
    /// <summary>
    /// Input field or transformation (composable)
    /// </summary>
    public required ProjectionField Input { get; init; }
    
    /// <summary>
    /// Additional parameters for operations like Substring(start, length)
    /// </summary>
    public IDictionary<string, object?>? Parameters { get; init; }
    
    public override Type GetOutputType() => 
        Operation switch
        {
            StringOperation.Length => typeof(int),
            _ => typeof(string)
        };
}

/// <summary>
/// Concatenate multiple fields/transformations into a single string
/// </summary>
public sealed record ConcatTransform : ProjectionField
{
    /// <summary>
    /// Fields or transformations to concatenate (all composable)
    /// </summary>
    public required IReadOnlyList<ProjectionField> Inputs { get; init; }
    
    /// <summary>
    /// Optional separator between concatenated values
    /// </summary>
    public string? Separator { get; init; }
    
    public override Type GetOutputType() => typeof(string);
}

/// <summary>
/// Arithmetic operations (Add, Subtract, Multiply, Divide, etc.)
/// </summary>
public sealed record ArithmeticTransform : ProjectionField
{
    /// <summary>
    /// Operation type
    /// </summary>
    public required ArithmeticOperation Operation { get; init; }
    
    /// <summary>
    /// Left operand (composable)
    /// </summary>
    public required ProjectionField Left { get; init; }
    
    /// <summary>
    /// Right operand (composable)
    /// </summary>
    public required ProjectionField Right { get; init; }
    
    public override Type GetOutputType() => typeof(decimal);
}

/// <summary>
/// Conditional expression (CASE WHEN ... THEN ... ELSE ...)
/// </summary>
public sealed record ConditionalTransform : ProjectionField
{
    /// <summary>
    /// Condition branches (evaluated in order)
    /// </summary>
    public required IReadOnlyList<ConditionalBranch> Branches { get; init; }
    
    /// <summary>
    /// Else branch (default if no conditions match)
    /// </summary>
    public required ProjectionField ElseBranch { get; init; }
    
    public override Type GetOutputType() => typeof(object);
}

/// <summary>
/// Single branch in a conditional expression
/// </summary>
public sealed record ConditionalBranch
{
    /// <summary>
    /// Condition to evaluate (composable)
    /// </summary>
    public required ProjectionField Condition { get; init; }
    
    /// <summary>
    /// Value to return if condition is true (composable)
    /// </summary>
    public required ProjectionField ThenValue { get; init; }
}

/// <summary>
/// Comparison operation for conditionals
/// </summary>
public sealed record ComparisonTransform : ProjectionField
{
    /// <summary>
    /// Comparison operator
    /// </summary>
    public required ComparisonOperator Operator { get; init; }
    
    /// <summary>
    /// Left operand (composable)
    /// </summary>
    public required ProjectionField Left { get; init; }
    
    /// <summary>
    /// Right operand (composable)
    /// </summary>
    public required ProjectionField Right { get; init; }
    
    public override Type GetOutputType() => typeof(bool);
}

/// <summary>
/// Coalesce operation - return first non-null value
/// </summary>
public sealed record CoalesceTransform : ProjectionField
{
    /// <summary>
    /// Values to check in order (all composable)
    /// </summary>
    public required IReadOnlyList<ProjectionField> Values { get; init; }
    
    public override Type GetOutputType() => typeof(object);
}

/// <summary>
/// Construct a nested object from multiple fields
/// </summary>
public sealed record ObjectConstruction : ProjectionField
{
    /// <summary>
    /// Properties to include in the constructed object
    /// </summary>
    public required IReadOnlyDictionary<string, ProjectionField> Properties { get; init; }
    
    public override Type GetOutputType() => typeof(object);
}

/// <summary>
/// Type cast operation
/// </summary>
public sealed record TypeCast : ProjectionField
{
    /// <summary>
    /// Input to cast (composable)
    /// </summary>
    public required ProjectionField Input { get; init; }
    
    /// <summary>
    /// Target type
    /// </summary>
    public required Type TargetType { get; init; }
    
    public override Type GetOutputType() => TargetType;
}

/// <summary>
/// String operations
/// </summary>
public enum StringOperation
{
    ToUpper,
    ToLower,
    Trim,
    TrimStart,
    TrimEnd,
    Substring,
    Length,
    Replace,
    PadLeft,
    PadRight
}

/// <summary>
/// Arithmetic operations
/// </summary>
public enum ArithmeticOperation
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    Round,
    Floor,
    Ceiling,
    Abs
}

/// <summary>
/// Comparison operators
/// </summary>
public enum ComparisonOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    IsNull,
    IsNotNull
}

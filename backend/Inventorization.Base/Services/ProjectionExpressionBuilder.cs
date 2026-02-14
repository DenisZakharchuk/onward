using System.Linq.Expressions;
using System.Reflection;
using Inventorization.Base.ADTs;
using Inventorization.Base.Abstractions;
using Inventorization.Base.Models;

namespace Inventorization.Base.Services;

/// <summary>
/// Builds LINQ expressions from ProjectionField transformations.
/// Supports recursive composition of transformations.
/// </summary>
public class ProjectionExpressionBuilder
{
    /// <summary>
    /// Builds a LINQ expression from a ProjectionField transformation
    /// </summary>
    /// <typeparam name="TEntity">Source entity type</typeparam>
    /// <param name="projectionField">The projection field transformation</param>
    /// <param name="parameter">The parameter expression representing the entity</param>
    /// <returns>An expression representing the transformation</returns>
    public Expression BuildExpression<TEntity>(ProjectionField projectionField, ParameterExpression parameter)
        where TEntity : class
    {
        return projectionField switch
        {
            FieldReference fieldRef => BuildFieldReference<TEntity>(fieldRef, parameter),
            ConstantValue constant => BuildConstant(constant),
            StringTransform stringTransform => BuildStringTransform<TEntity>(stringTransform, parameter),
            ConcatTransform concatTransform => BuildConcat<TEntity>(concatTransform, parameter),
            ArithmeticTransform arithmeticTransform => BuildArithmetic<TEntity>(arithmeticTransform, parameter),
            ComparisonTransform comparisonTransform => BuildComparison<TEntity>(comparisonTransform, parameter),
            ConditionalTransform conditionalTransform => BuildConditional<TEntity>(conditionalTransform, parameter),
            CoalesceTransform coalesceTransform => BuildCoalesce<TEntity>(coalesceTransform, parameter),
            ObjectConstruction objectConstruction => BuildObjectConstruction<TEntity>(objectConstruction, parameter),
            TypeCast typeCast => BuildTypeCast<TEntity>(typeCast, parameter),
            _ => throw new NotSupportedException($"Projection field type {projectionField.GetType().Name} is not supported")
        };
    }

    /// <summary>
    /// Builds a complete projection expression that transforms an entity to TransformationResult.
    /// Infers the schema from ProjectionField type information and creates a type-safe result.
    /// </summary>
    /// <typeparam name="TEntity">Source entity type</typeparam>
    /// <param name="transformations">Dictionary of output field names to projection transformations</param>
    /// <returns>Expression that converts TEntity to TransformationResult</returns>
    public Expression<Func<TEntity, TransformationResult>> BuildTransformationExpression<TEntity>(
        IReadOnlyDictionary<string, ProjectionField> transformations)
        where TEntity : class
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        
        // Step 1: Infer schema from transformation output types
        var schema = new Dictionary<string, Type>();
        foreach (var kvp in transformations)
        {
            var outputType = kvp.Value.GetOutputType();
            schema[kvp.Key] = outputType;
        }
        
        // Step 2: Create schema expression
        var schemaVar = Expression.Variable(typeof(Dictionary<string, Type>), "schema");
        var createSchema = Expression.Assign(schemaVar, 
            Expression.New(typeof(Dictionary<string, Type>)));
        
        var statements = new List<Expression> { createSchema };
        
        // Add each field to schema
        var addMethod = typeof(Dictionary<string, Type>).GetMethod("Add")!;
        foreach (var kvp in schema)
        {
            statements.Add(Expression.Call(
                schemaVar,
                addMethod,
                Expression.Constant(kvp.Key),
                Expression.Constant(kvp.Value, typeof(Type))
            ));
        }
        
        // Step 3: Create TransformationResult instance
        var resultVar = Expression.Variable(typeof(TransformationResult), "result");
        var ctor = typeof(TransformationResult).GetConstructor(new[] { typeof(Dictionary<string, Type>) })!;
        var createResult = Expression.Assign(resultVar, Expression.New(ctor, schemaVar));
        statements.Add(createResult);
        
        // Step 4: Build and add each transformation value
        // Use Dictionary's Add method instead of indexer to avoid ambiguity
        var dictionaryAddMethod = typeof(Dictionary<string, object?>).GetMethod("Add")!;
        foreach (var kvp in transformations)
        {
            var fieldName = kvp.Key;
            var transformation = kvp.Value;

            // Build the transformation expression
            var valueExpr = BuildExpression<TEntity>(transformation, parameter);
            
            // Convert to object if necessary
            var valueAsObject = valueExpr.Type == typeof(object) 
                ? valueExpr 
                : Expression.Convert(valueExpr, typeof(object));

            // Add to dictionary: result.Add(fieldName, value)
            var addCall = Expression.Call(
                resultVar,
                dictionaryAddMethod,
                Expression.Constant(fieldName),
                valueAsObject
            );

            statements.Add(addCall);
        }

        // Step 5: Return the result
        statements.Add(resultVar);

        var block = Expression.Block(
            new[] { schemaVar, resultVar },
            statements
        );

        return Expression.Lambda<Func<TEntity, TransformationResult>>(block, parameter);
    }

    private Expression BuildFieldReference<TEntity>(FieldReference fieldRef, ParameterExpression parameter)
        where TEntity : class
    {
        // Handle nested properties (e.g., "Category.Name")
        var propertyNames = fieldRef.FieldName.Split('.');
        Expression current = parameter;

        foreach (var propertyName in propertyNames)
        {
            var propertyInfo = current.Type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propertyInfo == null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' not found on type '{current.Type.Name}'");
            }

            current = Expression.Property(current, propertyInfo);
        }

        return current;
    }

    private Expression BuildConstant(ConstantValue constant)
    {
        return Expression.Constant(constant.Value, constant.ValueType ?? constant.Value?.GetType() ?? typeof(object));
    }

    private Expression BuildStringTransform<TEntity>(StringTransform transform, ParameterExpression parameter)
        where TEntity : class
    {
        var input = BuildExpression<TEntity>(transform.Input, parameter);

        // Ensure input is string type
        if (input.Type != typeof(string))
        {
            // Try to convert to string
            input = Expression.Call(input, typeof(object).GetMethod(nameof(ToString))!);
        }

        return transform.Operation switch
        {
            StringOperation.ToUpper => Expression.Call(input, typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!),
            StringOperation.ToLower => Expression.Call(input, typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!),
            StringOperation.Trim => Expression.Call(input, typeof(string).GetMethod(nameof(string.Trim), Type.EmptyTypes)!),
            StringOperation.TrimStart => Expression.Call(input, typeof(string).GetMethod(nameof(string.TrimStart), Type.EmptyTypes)!),
            StringOperation.TrimEnd => Expression.Call(input, typeof(string).GetMethod(nameof(string.TrimEnd), Type.EmptyTypes)!),
            StringOperation.Length => Expression.Property(input, typeof(string).GetProperty(nameof(string.Length))!),
            StringOperation.Substring => BuildSubstring<TEntity>(input, transform.Parameters, parameter),
            StringOperation.Replace => BuildReplace<TEntity>(input, transform.Parameters, parameter),
            StringOperation.PadLeft => BuildPad<TEntity>(input, transform.Parameters, true, parameter),
            StringOperation.PadRight => BuildPad<TEntity>(input, transform.Parameters, false, parameter),
            _ => throw new NotSupportedException($"String operation {transform.Operation} is not supported")
        };
    }

    private Expression BuildSubstring<TEntity>(Expression input, IDictionary<string, object?>? parameters, ParameterExpression parameter)
        where TEntity : class
    {
        if (parameters == null)
        {
            throw new InvalidOperationException("Substring requires 'start' parameter");
        }

        var start = parameters.TryGetValue("start", out var startObj) ? Convert.ToInt32(startObj) : 0;
        var startExpr = Expression.Constant(start);

        if (parameters.TryGetValue("length", out var lengthObj))
        {
            var length = Convert.ToInt32(lengthObj);
            var lengthExpr = Expression.Constant(length);
            var method = typeof(string).GetMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) })!;
            return Expression.Call(input, method, startExpr, lengthExpr);
        }
        else
        {
            var method = typeof(string).GetMethod(nameof(string.Substring), new[] { typeof(int) })!;
            return Expression.Call(input, method, startExpr);
        }
    }

    private Expression BuildReplace<TEntity>(Expression input, IDictionary<string, object?>? parameters, ParameterExpression parameter)
        where TEntity : class
    {
        if (parameters == null || !parameters.ContainsKey("oldValue") || !parameters.ContainsKey("newValue"))
        {
            throw new InvalidOperationException("Replace requires 'oldValue' and 'newValue' parameters");
        }

        var oldValue = Expression.Constant(parameters["oldValue"]?.ToString() ?? string.Empty);
        var newValue = Expression.Constant(parameters["newValue"]?.ToString() ?? string.Empty);
        var method = typeof(string).GetMethod(nameof(string.Replace), new[] { typeof(string), typeof(string) })!;

        return Expression.Call(input, method, oldValue, newValue);
    }

    private Expression BuildPad<TEntity>(Expression input, IDictionary<string, object?>? parameters, bool left, ParameterExpression parameter)
        where TEntity : class
    {
        if (parameters == null || !parameters.ContainsKey("width"))
        {
            throw new InvalidOperationException($"Pad{(left ? "Left" : "Right")} requires 'width' parameter");
        }

        var width = Expression.Constant(Convert.ToInt32(parameters["width"]));
        var methodName = left ? nameof(string.PadLeft) : nameof(string.PadRight);
        var method = typeof(string).GetMethod(methodName, new[] { typeof(int) })!;

        return Expression.Call(input, method, width);
    }

    private Expression BuildConcat<TEntity>(ConcatTransform concat, ParameterExpression parameter)
        where TEntity : class
    {
        var inputs = concat.Inputs.Select(input => BuildExpression<TEntity>(input, parameter)).ToList();

        // Convert all inputs to strings
        var stringInputs = inputs.Select(input =>
        {
            if (input.Type == typeof(string))
                return input;
            
            // Call ToString() on non-string types
            return Expression.Call(input, typeof(object).GetMethod(nameof(ToString))!);
        }).ToArray();

        if (concat.Separator != null)
        {
            // Use string.Join with separator
            var separatorExpr = Expression.Constant(concat.Separator);
            var arrayExpr = Expression.NewArrayInit(typeof(string), stringInputs);
            var method = typeof(string).GetMethod(nameof(string.Join), new[] { typeof(string), typeof(string[]) })!;
            return Expression.Call(method, separatorExpr, arrayExpr);
        }
        else
        {
            // Use string.Concat
            var arrayExpr = Expression.NewArrayInit(typeof(string), stringInputs);
            var method = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string[]) })!;
            return Expression.Call(method, arrayExpr);
        }
    }

    private Expression BuildArithmetic<TEntity>(ArithmeticTransform arithmetic, ParameterExpression parameter)
        where TEntity : class
    {
        var left = BuildExpression<TEntity>(arithmetic.Left, parameter);
        var right = BuildExpression<TEntity>(arithmetic.Right, parameter);

        // Convert to decimal if not numeric
        left = ConvertToNumeric(left);
        right = ConvertToNumeric(right);

        return arithmetic.Operation switch
        {
            ArithmeticOperation.Add => Expression.Add(left, right),
            ArithmeticOperation.Subtract => Expression.Subtract(left, right),
            ArithmeticOperation.Multiply => Expression.Multiply(left, right),
            ArithmeticOperation.Divide => Expression.Divide(left, right),
            ArithmeticOperation.Modulo => Expression.Modulo(left, right),
            ArithmeticOperation.Round => BuildRound(left),
            ArithmeticOperation.Floor => BuildFloor(left),
            ArithmeticOperation.Ceiling => BuildCeiling(left),
            ArithmeticOperation.Abs => BuildAbs(left),
            _ => throw new NotSupportedException($"Arithmetic operation {arithmetic.Operation} is not supported")
        };
    }

    private Expression ConvertToNumeric(Expression expr)
    {
        if (expr.Type == typeof(decimal) || expr.Type == typeof(int) || expr.Type == typeof(double) || expr.Type == typeof(float) || expr.Type == typeof(long))
        {
            return expr;
        }

        // Convert to decimal
        return Expression.Convert(expr, typeof(decimal));
    }

    private Expression BuildRound(Expression input)
    {
        var method = typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(decimal) })!;
        return Expression.Call(method, ConvertToNumeric(input));
    }

    private Expression BuildFloor(Expression input)
    {
        var method = typeof(Math).GetMethod(nameof(Math.Floor), new[] { typeof(decimal) })!;
        return Expression.Call(method, ConvertToNumeric(input));
    }

    private Expression BuildCeiling(Expression input)
    {
        var method = typeof(Math).GetMethod(nameof(Math.Ceiling), new[] { typeof(decimal) })!;
        return Expression.Call(method, ConvertToNumeric(input));
    }

    private Expression BuildAbs(Expression input)
    {
        var method = typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(decimal) })!;
        return Expression.Call(method, ConvertToNumeric(input));
    }

    private Expression BuildComparison<TEntity>(ComparisonTransform comparison, ParameterExpression parameter)
        where TEntity : class
    {
        var left = BuildExpression<TEntity>(comparison.Left, parameter);
        var right = BuildExpression<TEntity>(comparison.Right, parameter);

        return comparison.Operator switch
        {
            ComparisonOperator.Equal => Expression.Equal(left, right),
            ComparisonOperator.NotEqual => Expression.NotEqual(left, right),
            ComparisonOperator.GreaterThan => Expression.GreaterThan(left, right),
            ComparisonOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(left, right),
            ComparisonOperator.LessThan => Expression.LessThan(left, right),
            ComparisonOperator.LessThanOrEqual => Expression.LessThanOrEqual(left, right),
            ComparisonOperator.IsNull => Expression.Equal(left, Expression.Constant(null)),
            ComparisonOperator.IsNotNull => Expression.NotEqual(left, Expression.Constant(null)),
            _ => throw new NotSupportedException($"Comparison operator {comparison.Operator} is not supported")
        };
    }

    private Expression BuildConditional<TEntity>(ConditionalTransform conditional, ParameterExpression parameter)
        where TEntity : class
    {
        // Build CASE WHEN using nested ternary expressions
        var elseBranch = BuildExpression<TEntity>(conditional.ElseBranch, parameter);

        // Work backwards from the else branch
        Expression result = elseBranch;
        for (int i = conditional.Branches.Count - 1; i >= 0; i--)
        {
            var branch = conditional.Branches[i];
            var condition = BuildExpression<TEntity>(branch.Condition, parameter);
            var thenValue = BuildExpression<TEntity>(branch.ThenValue, parameter);

            // Ensure condition is boolean
            if (condition.Type != typeof(bool))
            {
                throw new InvalidOperationException($"Condition must evaluate to boolean, got {condition.Type.Name}");
            }

            result = Expression.Condition(condition, thenValue, result);
        }

        return result;
    }

    private Expression BuildCoalesce<TEntity>(CoalesceTransform coalesce, ParameterExpression parameter)
        where TEntity : class
    {
        if (coalesce.Values.Count == 0)
        {
            throw new InvalidOperationException("Coalesce requires at least one value");
        }

        var expressions = coalesce.Values.Select(v => BuildExpression<TEntity>(v, parameter)).ToList();

        // Build nested coalesce expressions
        Expression result = expressions[^1]; // Last value as fallback
        for (int i = expressions.Count - 2; i >= 0; i--)
        {
            result = Expression.Coalesce(expressions[i], result);
        }

        return result;
    }

    private Expression BuildObjectConstruction<TEntity>(ObjectConstruction objConstruction, ParameterExpression parameter)
        where TEntity : class
    {
        // Build an anonymous type or ExpandoObject
        // For simplicity, we'll create a Dictionary<string, object?>
        var properties = new List<ElementInit>();

        foreach (var kvp in objConstruction.Properties)
        {
            var valueExpr = BuildExpression<TEntity>(kvp.Value, parameter);
            var keyExpr = Expression.Constant(kvp.Key);
            
            // Convert value to object
            var valueAsObject = Expression.Convert(valueExpr, typeof(object));

            // Dictionary.Add(key, value)
            var addMethod = typeof(Dictionary<string, object?>).GetMethod(nameof(Dictionary<string, object?>.Add))!;
            properties.Add(Expression.ElementInit(addMethod, keyExpr, valueAsObject));
        }

        var dictType = typeof(Dictionary<string, object?>);
        var ctor = dictType.GetConstructor(Type.EmptyTypes)!;
        
        return Expression.ListInit(Expression.New(ctor), properties);
    }

    private Expression BuildTypeCast<TEntity>(TypeCast typeCast, ParameterExpression parameter)
        where TEntity : class
    {
        var input = BuildExpression<TEntity>(typeCast.Input, parameter);
        return Expression.Convert(input, typeCast.TargetType);
    }
}

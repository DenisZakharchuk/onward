using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inventorization.Base.ADTs.Converters;

/// <summary>
/// JSON converter for ProjectionField with recursive composite transformation support
/// Supports both explicit and short-form syntax
/// </summary>
public class ProjectionFieldConverter : JsonConverter<ProjectionField>
{
    public override ProjectionField? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        // Simple string = field reference
        if (reader.TokenType == JsonTokenType.String)
        {
            return new FieldReference { FieldName = reader.GetString()! };
        }

        // Number = constant
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out var intValue))
            {
                return new ConstantValue { Value = intValue, ValueType = typeof(int) };
            }
            if (reader.TryGetDecimal(out var decValue))
            {
                return new ConstantValue { Value = decValue, ValueType = typeof(decimal) };
            }
            return new ConstantValue { Value = reader.GetDouble(), ValueType = typeof(double) };
        }

        // Boolean = constant
        if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
        {
            return new ConstantValue { Value = reader.GetBoolean(), ValueType = typeof(bool) };
        }

        // Object = operation or field reference with metadata
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ParseObject(doc.RootElement, options);
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    private ProjectionField ParseObject(JsonElement element, JsonSerializerOptions options)
    {
        // Check for specific operation keys
        if (element.TryGetProperty("field", out var fieldElement))
        {
            return new FieldReference { FieldName = fieldElement.GetString()! };
        }

        if (element.TryGetProperty("const", out var constElement))
        {
            return ParseConstant(constElement);
        }

        // String operations
        if (element.TryGetProperty("toUpper", out var toUpperElement))
        {
            return new StringTransform
            {
                Operation = StringOperation.ToUpper,
                Input = ParseProjectionField(toUpperElement, options)
            };
        }

        if (element.TryGetProperty("toLower", out var toLowerElement))
        {
            return new StringTransform
            {
                Operation = StringOperation.ToLower,
                Input = ParseProjectionField(toLowerElement, options)
            };
        }

        if (element.TryGetProperty("trim", out var trimElement))
        {
            return new StringTransform
            {
                Operation = StringOperation.Trim,
                Input = ParseProjectionField(trimElement, options)
            };
        }

        if (element.TryGetProperty("substring", out var substringElement))
        {
            return ParseSubstring(substringElement, options);
        }

        if (element.TryGetProperty("length", out var lengthElement))
        {
            return new StringTransform
            {
                Operation = StringOperation.Length,
                Input = ParseProjectionField(lengthElement, options)
            };
        }

        if (element.TryGetProperty("replace", out var replaceElement))
        {
            return ParseReplace(replaceElement, options);
        }

        if (element.TryGetProperty("concat", out var concatElement))
        {
            return ParseConcat(concatElement, options);
        }

        // Arithmetic operations
        if (element.TryGetProperty("add", out var addElement))
        {
            return ParseBinaryArithmetic(ArithmeticOperation.Add, addElement, options);
        }

        if (element.TryGetProperty("subtract", out var subtractElement))
        {
            return ParseBinaryArithmetic(ArithmeticOperation.Subtract, subtractElement, options);
        }

        if (element.TryGetProperty("multiply", out var multiplyElement))
        {
            return ParseBinaryArithmetic(ArithmeticOperation.Multiply, multiplyElement, options);
        }

        if (element.TryGetProperty("divide", out var divideElement))
        {
            return ParseBinaryArithmetic(ArithmeticOperation.Divide, divideElement, options);
        }

        if (element.TryGetProperty("round", out var roundElement))
        {
            return new ArithmeticTransform
            {
                Operation = ArithmeticOperation.Round,
                Left = ParseProjectionField(roundElement, options),
                Right = new ConstantValue { Value = 0, ValueType = typeof(int) } // Default precision
            };
        }

        // Comparison operations
        if (element.TryGetProperty("eq", out var eqElement))
        {
            return ParseBinaryComparison(ComparisonOperator.Equal, eqElement, options);
        }

        if (element.TryGetProperty("gt", out var gtElement))
        {
            return ParseBinaryComparison(ComparisonOperator.GreaterThan, gtElement, options);
        }

        if (element.TryGetProperty("gte", out var gteElement))
        {
            return ParseBinaryComparison(ComparisonOperator.GreaterThanOrEqual, gteElement, options);
        }

        if (element.TryGetProperty("lt", out var ltElement))
        {
            return ParseBinaryComparison(ComparisonOperator.LessThan, ltElement, options);
        }

        if (element.TryGetProperty("lte", out var lteElement))
        {
            return ParseBinaryComparison(ComparisonOperator.LessThanOrEqual, lteElement, options);
        }

        if (element.TryGetProperty("isNull", out var isNullElement))
        {
            return new ComparisonTransform
            {
                Operator = ComparisonOperator.IsNull,
                Left = ParseProjectionField(isNullElement, options),
                Right = new ConstantValue { Value = null }
            };
        }

        // Conditional (case/when)
        if (element.TryGetProperty("case", out var caseElement))
        {
            return ParseConditional(caseElement, options);
        }

        // Coalesce
        if (element.TryGetProperty("coalesce", out var coalesceElement))
        {
            return ParseCoalesce(coalesceElement, options);
        }

        // Object construction
        if (element.TryGetProperty("object", out var objectElement))
        {
            return ParseObjectConstruction(objectElement, options);
        }

        throw new JsonException($"Unknown projection operation in object: {element}");
    }

    private ProjectionField ParseProjectionField(JsonElement element, JsonSerializerOptions options)
    {
        // String shorthand = field reference
        if (element.ValueKind == JsonValueKind.String)
        {
            return new FieldReference { FieldName = element.GetString()! };
        }

        // Number = constant
        if (element.ValueKind == JsonValueKind.Number)
        {
            if (element.TryGetInt32(out var intValue))
            {
                return new ConstantValue { Value = intValue, ValueType = typeof(int) };
            }
            if (element.TryGetDecimal(out var decValue))
            {
                return new ConstantValue { Value = decValue, ValueType = typeof(decimal) };
            }
            return new ConstantValue { Value = element.GetDouble(), ValueType = typeof(double) };
        }

        // Boolean = constant
        if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
        {
            return new ConstantValue { Value = element.GetBoolean(), ValueType = typeof(bool) };
        }

        // Null = constant
        if (element.ValueKind == JsonValueKind.Null)
        {
            return new ConstantValue { Value = null };
        }

        // Object = nested operation
        if (element.ValueKind == JsonValueKind.Object)
        {
            return ParseObject(element, options);
        }

        throw new JsonException($"Unexpected JSON element kind: {element.ValueKind}");
    }

    private ConstantValue ParseConstant(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => new ConstantValue { Value = element.GetString(), ValueType = typeof(string) },
            JsonValueKind.Number => ParseNumberConstant(element),
            JsonValueKind.True => new ConstantValue { Value = true, ValueType = typeof(bool) },
            JsonValueKind.False => new ConstantValue { Value = false, ValueType = typeof(bool) },
            JsonValueKind.Null => new ConstantValue { Value = null },
            _ => throw new JsonException($"Unsupported constant type: {element.ValueKind}")
        };
    }

    private ConstantValue ParseNumberConstant(JsonElement element)
    {
        if (element.TryGetInt32(out var intValue))
        {
            return new ConstantValue { Value = intValue, ValueType = typeof(int) };
        }
        if (element.TryGetDecimal(out var decValue))
        {
            return new ConstantValue { Value = decValue, ValueType = typeof(decimal) };
        }
        return new ConstantValue { Value = element.GetDouble(), ValueType = typeof(double) };
    }

    private StringTransform ParseSubstring(JsonElement element, JsonSerializerOptions options)
    {
        var input = element.GetProperty("input");
        var start = element.GetProperty("start").GetInt32();
        var length = element.TryGetProperty("length", out var lengthElement) ? lengthElement.GetInt32() : (int?)null;

        var parameters = new Dictionary<string, object?>
        {
            ["start"] = start
        };
        if (length.HasValue)
        {
            parameters["length"] = length.Value;
        }

        return new StringTransform
        {
            Operation = StringOperation.Substring,
            Input = ParseProjectionField(input, options),
            Parameters = parameters
        };
    }

    private StringTransform ParseReplace(JsonElement element, JsonSerializerOptions options)
    {
        var input = element.GetProperty("input");
        var oldValue = element.GetProperty("oldValue").GetString()!;
        var newValue = element.GetProperty("newValue").GetString()!;

        return new StringTransform
        {
            Operation = StringOperation.Replace,
            Input = ParseProjectionField(input, options),
            Parameters = new Dictionary<string, object?>
            {
                ["oldValue"] = oldValue,
                ["newValue"] = newValue
            }
        };
    }

    private ConcatTransform ParseConcat(JsonElement element, JsonSerializerOptions options)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            // Short form: {"concat": ["field1", "field2"]}
            var inputs = new List<ProjectionField>();
            foreach (var item in element.EnumerateArray())
            {
                inputs.Add(ParseProjectionField(item, options));
            }
            return new ConcatTransform { Inputs = inputs };
        }

        // Long form: {"concat": {"inputs": [...], "separator": " "}}
        var inputsElement = element.GetProperty("inputs");
        var inputs2 = new List<ProjectionField>();
        foreach (var item in inputsElement.EnumerateArray())
        {
            inputs2.Add(ParseProjectionField(item, options));
        }

        var separator = element.TryGetProperty("separator", out var sepElement) ? sepElement.GetString() : null;

        return new ConcatTransform { Inputs = inputs2, Separator = separator };
    }

    private ArithmeticTransform ParseBinaryArithmetic(ArithmeticOperation operation, JsonElement element, JsonSerializerOptions options)
    {
        // Short form: {"add": ["field1", "field2"]} or {"add": {"left": ..., "right": ...}}
        if (element.ValueKind == JsonValueKind.Array)
        {
            var array = element.EnumerateArray().ToList();
            if (array.Count != 2)
            {
                throw new JsonException($"Arithmetic operation requires exactly 2 operands, got {array.Count}");
            }
            return new ArithmeticTransform
            {
                Operation = operation,
                Left = ParseProjectionField(array[0], options),
                Right = ParseProjectionField(array[1], options)
            };
        }

        var left = element.GetProperty("left");
        var right = element.GetProperty("right");

        return new ArithmeticTransform
        {
            Operation = operation,
            Left = ParseProjectionField(left, options),
            Right = ParseProjectionField(right, options)
        };
    }

    private ComparisonTransform ParseBinaryComparison(ComparisonOperator op, JsonElement element, JsonSerializerOptions options)
    {
        // Short form: {"eq": ["field1", "value"]} or {"eq": {"left": ..., "right": ...}}
        if (element.ValueKind == JsonValueKind.Array)
        {
            var array = element.EnumerateArray().ToList();
            if (array.Count != 2)
            {
                throw new JsonException($"Comparison operation requires exactly 2 operands, got {array.Count}");
            }
            return new ComparisonTransform
            {
                Operator = op,
                Left = ParseProjectionField(array[0], options),
                Right = ParseProjectionField(array[1], options)
            };
        }

        var left = element.GetProperty("left");
        var right = element.GetProperty("right");

        return new ComparisonTransform
        {
            Operator = op,
            Left = ParseProjectionField(left, options),
            Right = ParseProjectionField(right, options)
        };
    }

    private ConditionalTransform ParseConditional(JsonElement element, JsonSerializerOptions options)
    {
        var branches = new List<ConditionalBranch>();
        var whenElement = element.GetProperty("when");

        foreach (var branch in whenElement.EnumerateArray())
        {
            var condition = branch.GetProperty("condition");
            var then = branch.GetProperty("then");

            branches.Add(new ConditionalBranch
            {
                Condition = ParseProjectionField(condition, options),
                ThenValue = ParseProjectionField(then, options)
            });
        }

        var elseElement = element.GetProperty("else");

        return new ConditionalTransform
        {
            Branches = branches,
            ElseBranch = ParseProjectionField(elseElement, options)
        };
    }

    private CoalesceTransform ParseCoalesce(JsonElement element, JsonSerializerOptions options)
    {
        var values = new List<ProjectionField>();
        foreach (var item in element.EnumerateArray())
        {
            values.Add(ParseProjectionField(item, options));
        }

        return new CoalesceTransform { Values = values };
    }

    private ObjectConstruction ParseObjectConstruction(JsonElement element, JsonSerializerOptions options)
    {
        var properties = new Dictionary<string, ProjectionField>();

        foreach (var prop in element.EnumerateObject())
        {
            properties[prop.Name] = ParseProjectionField(prop.Value, options);
        }

        return new ObjectConstruction { Properties = properties };
    }

    public override void Write(Utf8JsonWriter writer, ProjectionField value, JsonSerializerOptions options)
    {
        // Serialization not needed for now (input only)
        throw new NotImplementedException("ProjectionField serialization not implemented");
    }
}

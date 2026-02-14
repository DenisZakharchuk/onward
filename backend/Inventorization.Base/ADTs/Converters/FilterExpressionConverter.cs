using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inventorization.Base.ADTs.Converters;

/// <summary>
/// Custom JSON converter for FilterExpression that supports MongoDB-style compact syntax.
/// 
/// Compact format examples:
/// - Simple equality: {"IsActive": true}
/// - Operator: {"UnitPrice": {"gt": 10}}
/// - Multiple operators: {"UnitPrice": {"gte": 10, "lte": 100}}
/// - AND (implicit): {"IsActive": true, "CategoryId": {"notNull": true}}
/// - OR (explicit): {"$or": [{"Name": {"contains": "widget"}}, {"Sku": {"startsWith": "PRE-"}}]}
/// - AND (explicit): {"$and": [{"IsActive": true}, {"UnitPrice": {"gt": 50}}]}
/// </summary>
public class FilterExpressionConverter : JsonConverter<FilterExpression?>
{
    private static readonly HashSet<string> OperatorKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "eq", "ne", "gt", "gte", "lt", "lte",
        "contains", "startsWith", "endsWith",
        "in", "notIn", "null", "notNull"
    };
    
    private static readonly HashSet<string> CombinatorKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "$and", "$or", "and", "or"
    };
    
    public override FilterExpression? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Filter must be a JSON object");
        
        using var doc = JsonDocument.ParseValue(ref reader);
        return ParseFilterObject(doc.RootElement);
    }
    
    public override void Write(Utf8JsonWriter writer, FilterExpression? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }
        
        // Write in compact format
        WriteCompactFilter(writer, value);
    }
    
    private FilterExpression ParseFilterObject(JsonElement element)
    {
        var properties = element.EnumerateObject().ToList();
        
        if (properties.Count == 0)
            throw new JsonException("Filter object cannot be empty");
        
        // Check for combinators ($and, $or)
        var combinatorProp = properties.FirstOrDefault(p => CombinatorKeys.Contains(p.Name));
        if (properties.Any(p => CombinatorKeys.Contains(p.Name)))
        {
            return ParseCombinator(combinatorProp.Name, combinatorProp.Value);
        }
        
        // Multiple properties = implicit AND
        if (properties.Count > 1)
        {
            var expressions = properties.Select(p => ParseFieldCondition(p.Name, p.Value)).ToList();
            return new AndFilter(expressions);
        }
        
        // Single property = field condition
        var prop = properties[0];
        return ParseFieldCondition(prop.Name, prop.Value);
    }
    
    private FilterExpression ParseCombinator(string combinator, JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
            throw new JsonException($"{combinator} must be an array");
        
        var expressions = value.EnumerateArray()
            .Select(ParseFilterObject)
            .ToList();
        
        return combinator.ToLowerInvariant() switch
        {
            "$and" or "and" => new AndFilter(expressions),
            "$or" or "or" => new OrFilter(expressions),
            _ => throw new JsonException($"Unknown combinator: {combinator}")
        };
    }
    
    private FilterExpression ParseFieldCondition(string fieldName, JsonElement value)
    {
        // Direct value = equality
        if (value.ValueKind != JsonValueKind.Object)
        {
            var equalsCondition = CreateEqualsCondition(fieldName, value);
            return new LeafFilter(equalsCondition);
        }
        
        // Operator object = parse operators
        var operators = value.EnumerateObject().ToList();
        
        if (operators.Count == 0)
            throw new JsonException($"Operator object for field '{fieldName}' cannot be empty");
        
        // Multiple operators on same field = implicit AND
        if (operators.Count > 1)
        {
            var expressions = operators.Select(op => 
                new LeafFilter(ParseOperator(fieldName, op.Name, op.Value))).ToList();
            return new AndFilter(expressions);
        }
        
        // Single operator
        var operatorProp = operators[0];
        var condition = ParseOperator(fieldName, operatorProp.Name, operatorProp.Value);
        return new LeafFilter(condition);
    }
    
    private FilterCondition ParseOperator(string fieldName, string operatorName, JsonElement value)
    {
        return operatorName.ToLowerInvariant() switch
        {
            "eq" => CreateEqualsCondition(fieldName, value),
            "gt" => new GreaterThanCondition(fieldName, GetValue(value)),
            "gte" => new GreaterThanOrEqualCondition(fieldName, GetValue(value)),
            "lt" => new LessThanCondition(fieldName, GetValue(value)),
            "lte" => new LessThanOrEqualCondition(fieldName, GetValue(value)),
            "contains" => new ContainsCondition(fieldName, value.GetString() ?? throw new JsonException("Contains requires string value")),
            "startswith" => new StartsWithCondition(fieldName, value.GetString() ?? throw new JsonException("StartsWith requires string value")),
            "in" => ParseInCondition(fieldName, value),
            "null" => new IsNullCondition(fieldName),
            "notnull" => new IsNotNullCondition(fieldName),
            _ => throw new JsonException($"Unknown operator: {operatorName}")
        };
    }
    
    private FilterCondition CreateEqualsCondition(string fieldName, JsonElement value)
    {
        return new EqualsCondition(fieldName, GetValue(value));
    }
    
    private FilterCondition ParseInCondition(string fieldName, JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
            throw new JsonException("'in' operator requires an array");
        
        var values = value.EnumerateArray().Select(GetValue).ToList();
        return new InCondition(fieldName, values);
    }
    
    private object GetValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            _ => element.GetRawText()
        };
    }
    
    private void WriteCompactFilter(Utf8JsonWriter writer, FilterExpression expression)
    {
        switch (expression)
        {
            case LeafFilter leaf:
                WriteLeafFilter(writer, leaf);
                break;
            
            case AndFilter and:
                WriteAndFilter(writer, and);
                break;
            
            case OrFilter or:
                writer.WriteStartObject();
                writer.WritePropertyName("$or");
                writer.WriteStartArray();
                foreach (var expr in or.Expressions)
                {
                    WriteCompactFilter(writer, expr);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
                break;
        }
    }
    
    private void WriteLeafFilter(Utf8JsonWriter writer, LeafFilter leaf)
    {
        writer.WriteStartObject();
        
        var condition = leaf.Condition;
        writer.WritePropertyName(condition.FieldName);
        
        switch (condition)
        {
            case EqualsCondition eq:
                WriteValue(writer, eq.Value);
                break;
            
            case GreaterThanCondition gt:
                writer.WriteStartObject();
                writer.WritePropertyName("gt");
                WriteValue(writer, gt.Value);
                writer.WriteEndObject();
                break;
            
            case LessThanCondition lt:
                writer.WriteStartObject();
                writer.WritePropertyName("lt");
                WriteValue(writer, lt.Value);
                writer.WriteEndObject();
                break;
            
            case GreaterThanOrEqualCondition gte:
                writer.WriteStartObject();
                writer.WritePropertyName("gte");
                WriteValue(writer, gte.Value);
                writer.WriteEndObject();
                break;
            
            case LessThanOrEqualCondition lte:
                writer.WriteStartObject();
                writer.WritePropertyName("lte");
                WriteValue(writer, lte.Value);
                writer.WriteEndObject();
                break;
            
            case ContainsCondition contains:
                writer.WriteStartObject();
                writer.WritePropertyName("contains");
                writer.WriteStringValue(contains.Value);
                writer.WriteEndObject();
                break;
            
            case StartsWithCondition startsWith:
                writer.WriteStartObject();
                writer.WritePropertyName("startsWith");
                writer.WriteStringValue(startsWith.Value);
                writer.WriteEndObject();
                break;
            
            case InCondition inCond:
                writer.WriteStartObject();
                writer.WritePropertyName("in");
                writer.WriteStartArray();
                foreach (var val in inCond.Values)
                {
                    WriteValue(writer, val);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
                break;
            
            case IsNullCondition:
                writer.WriteStartObject();
                writer.WritePropertyName("null");
                writer.WriteBooleanValue(true);
                writer.WriteEndObject();
                break;
            
            case IsNotNullCondition:
                writer.WriteStartObject();
                writer.WritePropertyName("notNull");
                writer.WriteBooleanValue(true);
                writer.WriteEndObject();
                break;
        }
        
        writer.WriteEndObject();
    }
    
    private void WriteAndFilter(Utf8JsonWriter writer, AndFilter and)
    {
        // Try to write as implicit AND (multiple fields in one object)
        var allLeafs = and.Expressions.All(e => e is LeafFilter);
        
        if (allLeafs)
        {
            writer.WriteStartObject();
            foreach (var expr in and.Expressions)
            {
                var leaf = (LeafFilter)expr;
                writer.WritePropertyName(leaf.Condition.FieldName);
                
                // Write condition value inline
                switch (leaf.Condition)
                {
                    case EqualsCondition eq:
                        WriteValue(writer, eq.Value);
                        break;
                    default:
                        // For non-equality, write operator object
                        WriteLeafFilter(writer, leaf);
                        break;
                }
            }
            writer.WriteEndObject();
        }
        else
        {
            // Explicit $and for complex cases
            writer.WriteStartObject();
            writer.WritePropertyName("$and");
            writer.WriteStartArray();
            foreach (var expr in and.Expressions)
            {
                WriteCompactFilter(writer, expr);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
    
    private void WriteValue(Utf8JsonWriter writer, object value)
    {
        switch (value)
        {
            case string s:
                writer.WriteStringValue(s);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case decimal d:
                writer.WriteNumberValue(d);
                break;
            case double db:
                writer.WriteNumberValue(db);
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case null:
                writer.WriteNullValue();
                break;
            default:
                writer.WriteStringValue(value.ToString());
                break;
        }
    }
}

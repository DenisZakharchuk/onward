using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inventorization.Base.ADTs.Converters;

/// <summary>
/// Custom JSON converter for SortRequest that supports compact object/array syntax.
/// 
/// Compact formats:
/// - Object: {"Name": "asc", "UnitPrice": "desc"}
/// - Array: [{"Name": "asc"}, {"UnitPrice": "desc"}]
/// 
/// Original format still supported:
/// - Object with fields array: {"fields": [{"fieldName": "Name", "direction": "Ascending"}, ...]}
/// </summary>
public class SortRequestConverter : JsonConverter<SortRequest?>
{
    public override SortRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        
        // Compact format: object with field-direction pairs
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            
            // Check if it's the original format with "fields" property
            if (root.TryGetProperty("fields", out var fieldsElement))
            {
                return ParseOriginalFormat(fieldsElement);
            }
            
            // Compact object format: {"Name": "asc", "Price": "desc"}
            var fields = new List<SortField>();
            foreach (var property in root.EnumerateObject())
            {
                var fieldName = property.Name;
                var direction = ParseDirection(property.Value.GetString());
                fields.Add(new SortField(fieldName, direction));
            }
            
            return new SortRequest(fields);
        }
        
        // Compact format: array of single-property objects
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var fields = new List<SortField>();
            
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    // Each array item is like {"Name": "asc"}
                    var property = item.EnumerateObject().First();
                    var fieldName = property.Name;
                    var direction = ParseDirection(property.Value.GetString());
                    fields.Add(new SortField(fieldName, direction));
                }
            }
            
            return new SortRequest(fields);
        }
        
        throw new JsonException("Sort must be an object with field-direction pairs or an array");
    }
    
    public override void Write(Utf8JsonWriter writer, SortRequest? value, JsonSerializerOptions options)
    {
        if (value == null || value.Fields.Count == 0)
        {
            writer.WriteNullValue();
            return;
        }
        
        // Write in compact object format
        writer.WriteStartObject();
        foreach (var field in value.Fields)
        {
            writer.WritePropertyName(field.FieldName);
            writer.WriteStringValue(field.Direction == SortDirection.Ascending ? "asc" : "desc");
        }
        writer.WriteEndObject();
    }
    
    private SortRequest ParseOriginalFormat(JsonElement fieldsElement)
    {
        var fields = new List<SortField>();
        
        foreach (var fieldElement in fieldsElement.EnumerateArray())
        {
            var fieldName = fieldElement.GetProperty("fieldName").GetString()!;
            var directionStr = fieldElement.GetProperty("direction").GetString()!;
            var direction = directionStr.Equals("Ascending", StringComparison.OrdinalIgnoreCase)
                ? SortDirection.Ascending
                : SortDirection.Descending;
            
            fields.Add(new SortField(fieldName, direction));
        }
        
        return new SortRequest(fields);
    }
    
    private SortDirection ParseDirection(string? direction)
    {
        if (string.IsNullOrEmpty(direction))
            return SortDirection.Ascending;
        
        return direction.ToLowerInvariant() switch
        {
            "asc" or "ascending" => SortDirection.Ascending,
            "desc" or "descending" => SortDirection.Descending,
            _ => throw new JsonException($"Invalid sort direction: {direction}. Use 'asc' or 'desc'")
        };
    }
}

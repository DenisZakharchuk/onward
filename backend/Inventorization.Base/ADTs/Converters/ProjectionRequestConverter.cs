using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inventorization.Base.ADTs.Converters;

/// <summary>
/// Custom JSON converter for ProjectionRequest that supports compact array syntax, "all" option, and field transformations.
/// 
/// Compact formats:
/// - Simple array: ["Id", "Name", "Sku", "UnitPrice"]
/// - With related entities: ["Id", "Name", "Category.Name"]
/// - All fields (non-deep): {"all": {"deep": false}}
/// - All fields (deep): {"all": {"deep": true, "depth": 2}}
/// - With transformations: {"transform": {"upperName": {"toUpper": "Name"}, "total": {"multiply": ["UnitPrice", "QuantityInStock"]}}}
/// 
/// Original format still supported:
/// - Object with fields array: {"fields": [{"fieldName": "Id", "isRelated": false}, ...]}
/// </summary>
public class ProjectionRequestConverter : JsonConverter<ProjectionRequest?>
{
    public override ProjectionRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        
        // Compact format: array of strings
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var fields = new List<FieldProjection>();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;
                
                if (reader.TokenType == JsonTokenType.String)
                {
                    var fieldName = reader.GetString()!;
                    fields.Add(ParseFieldName(fieldName));
                }
            }
            
            return new ProjectionRequest(fields, isAllFields: false, includeRelatedDeep: false, depth: ProjectionRequest.DefaultDepth);
        }
        
        // Object format
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            
            // Check for "all" property
            if (root.TryGetProperty("all", out var allElement))
            {
                var deep = false;
                var depth = ProjectionRequest.DefaultDepth;
                
                // Support both "deep" (new) and "recursive" (legacy) property names
                if (allElement.TryGetProperty("deep", out var deepElement))
                {
                    deep = deepElement.GetBoolean();
                }
                else if (allElement.TryGetProperty("recursive", out var recursiveElement))
                {
                    // Legacy support
                    deep = recursiveElement.GetBoolean();
                }
                
                // Parse depth if provided
                if (allElement.TryGetProperty("depth", out var depthElement))
                {
                    depth = depthElement.GetInt32();
                }
                
                return new ProjectionRequest(
                    Array.Empty<FieldProjection>(), 
                    isAllFields: true, 
                    includeRelatedDeep: deep,
                    depth: depth);
            }
            
            // Check for "transform" property (field transformations)
            if (root.TryGetProperty("transform", out var transformElement))
            {
                var fieldTransformations = new Dictionary<string, ProjectionField>();
                var converter = new ProjectionFieldConverter();
                
                foreach (var prop in transformElement.EnumerateObject())
                {
                    var fieldName = prop.Name;
                    var transformJson = prop.Value.GetRawText();
                    
                    // Parse the transformation using ProjectionFieldConverter
                    var projectionField = JsonSerializer.Deserialize<ProjectionField>(transformJson, new JsonSerializerOptions
                    {
                        Converters = { converter }
                    });
                    
                    if (projectionField != null)
                    {
                        fieldTransformations[fieldName] = projectionField;
                    }
                }
                
                return new ProjectionRequest(
                    Array.Empty<FieldProjection>(),
                    isAllFields: false,
                    includeRelatedDeep: false,
                    depth: ProjectionRequest.DefaultDepth,
                    fieldTransformations: fieldTransformations);
            }
            
            // Original format with "fields" property
            if (root.TryGetProperty("fields", out var fieldsElement))
            {
                var fields = new List<FieldProjection>();
                
                foreach (var fieldElement in fieldsElement.EnumerateArray())
                {
                    var fieldName = fieldElement.GetProperty("fieldName").GetString()!;
                    var isRelated = fieldElement.TryGetProperty("isRelated", out var isRelatedProp) 
                        && isRelatedProp.GetBoolean();
                    var relationPath = fieldElement.TryGetProperty("relationPath", out var relationPathProp)
                        ? relationPathProp.GetString()
                        : null;
                    
                    fields.Add(new FieldProjection(fieldName, isRelated, relationPath));
                }
                
                return new ProjectionRequest(fields, isAllFields: false, includeRelatedDeep: false, depth: ProjectionRequest.DefaultDepth);
            }
        }
        
        throw new JsonException("Projection must be an array of field names, an object with 'all' property, an object with 'transform' property, or an object with 'fields' property");
    }
    
    public override void Write(Utf8JsonWriter writer, ProjectionRequest? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }
        
        // If FieldTransformations are specified, write transform format
        if (value.FieldTransformations != null && value.FieldTransformations.Count > 0)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("transform");
            writer.WriteStartObject();
            
            foreach (var kvp in value.FieldTransformations)
            {
                writer.WritePropertyName(kvp.Key);
                // Note: ProjectionFieldConverter Write is not implemented yet
                // For now, write a placeholder
                writer.WriteStringValue($"[Transformation: {kvp.Value.GetType().Name}]");
            }
            
            writer.WriteEndObject();
            writer.WriteEndObject();
            return;
        }
        
        // If IsAllFields, write compact "all" format
        if (value.IsAllFields)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("all");
            writer.WriteStartObject();
            writer.WritePropertyName("deep");
            writer.WriteBooleanValue(value.IncludeRelatedDeep);
            if (value.IncludeRelatedDeep && value.Depth != ProjectionRequest.DefaultDepth)
            {
                writer.WritePropertyName("depth");
                writer.WriteNumberValue(value.Depth);
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
            return;
        }
        
        // If no fields specified, return null
        if (value.Fields.Count == 0)
        {
            writer.WriteNullValue();
            return;
        }
        
        // Write in compact array format
        writer.WriteStartArray();
        foreach (var field in value.Fields)
        {
            writer.WriteStringValue(field.FieldName);
        }
        writer.WriteEndArray();
    }
    
    private FieldProjection ParseFieldName(string fieldName)
    {
        // Detect related fields by presence of dot (e.g., "Category.Name")
        if (fieldName.Contains('.'))
        {
            var parts = fieldName.Split('.', 2);
            var relationPath = parts[0];
            return new FieldProjection(fieldName, isRelated: true, relationPath: relationPath);
        }
        
        return new FieldProjection(fieldName, isRelated: false, relationPath: null);
    }
}

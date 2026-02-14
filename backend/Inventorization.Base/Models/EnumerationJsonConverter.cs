using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inventorization.Base.Models;

/// <summary>
/// JSON converter for Enumeration types.
/// Serializes as string name for API responses, deserializes from string or int.
/// </summary>
public class EnumerationJsonConverter<TEnumeration> : JsonConverter<TEnumeration>
    where TEnumeration : Enumeration
{
    public override TEnumeration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => Enumeration.FromNameOrThrow<TEnumeration>(reader.GetString()!),
            JsonTokenType.Number => Enumeration.FromValueOrThrow<TEnumeration>(reader.GetInt32()),
            _ => throw new JsonException($"Unable to convert {reader.TokenType} to {typeof(TEnumeration).Name}")
        };
    }

    public override void Write(Utf8JsonWriter writer, TEnumeration value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}

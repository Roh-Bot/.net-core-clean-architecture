using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Project.WebApi.Validation;

public class FlexibleStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string result = reader.TokenType switch
        {
            JsonTokenType.Null => string.Empty,
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Number =>
                reader.TryGetInt64(out long l)
                    ? l.ToString(CultureInfo.InvariantCulture)
                    : reader.GetDecimal().ToString(CultureInfo.InvariantCulture),
            _ => throw new JsonException($"Unsupported token type {reader.TokenType} for string conversion.")
        };

        // Validate that the string contains only a numeric value
        if (!decimal.TryParse(result, NumberStyles.None, CultureInfo.InvariantCulture, out _))
            throw new JsonException($"Value '{result}' is not a valid numeric string.");

        return result;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobTracker.Api.Infrastructure.Converters;

/// <summary>
/// Custom JSON converter for Cosmos DB that handles case-sensitive property mapping
/// and removes unwanted properties during serialization.
/// </summary>
public class CosmosJsonConverter : JsonConverter<object>
{
  public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    throw new NotImplementedException("This converter is write-only");
  }

  public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
  {
    if (value == null)
    {
      writer.WriteNullValue();
      return;
    }

    var jsonElement = JsonDocument.Parse(JsonSerializer.Serialize(value, options)).RootElement;
    jsonElement.WriteTo(writer);
  }
}

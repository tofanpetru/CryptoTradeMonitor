using Domain.Enums;
using Newtonsoft.Json;

namespace Infrastructure.Data.Converters
{
    public class PermissionTypeConverter : JsonConverter<PermissionType>
    {
        public override PermissionType ReadJson(JsonReader reader, Type objectType, PermissionType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string value)
            {
                if (Enum.TryParse<PermissionType>(value, out var result))
                {
                    return result;
                }
            }

            throw new JsonSerializationException($"Invalid permission type value: {reader.Value}");
        }

        public override void WriteJson(JsonWriter writer, PermissionType value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}

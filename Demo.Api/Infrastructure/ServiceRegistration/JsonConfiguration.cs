using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Demo.Api.Infrastructure.ServiceRegistration
{
    public static class JsonConfiguration
    {
        public static void ConfigureSystemTextJson(JsonSerializerOptions serializerOptions)
        {
            // Configures JsonSerializer to properly serialize NodaTime types.
            serializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            serializerOptions.Converters.Add(new JsonStringEnumConverter());
            serializerOptions.NumberHandling =
                JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString;

            //serializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        }
    }
}
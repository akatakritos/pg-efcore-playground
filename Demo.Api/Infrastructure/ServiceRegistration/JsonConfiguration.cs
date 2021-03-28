using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using Demo.Api.Domain;
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
            serializerOptions.Converters.Add(new SmartEnumNameConverter<UnitOfMeasure, int>());
            serializerOptions.NumberHandling =
                JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString;

            //serializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        }
    }
}

// inlined from https://github.com/ardalis/SmartEnum/blob/master/src/SmartEnum.SystemTextJson/SmartEnumNameConverter.cs
// due to https://github.com/ardalis/SmartEnum/issues/113
namespace Ardalis.SmartEnum.SystemTextJson
{
    public class SmartEnumNameConverter<TEnum, TValue> : JsonConverter<TEnum>
        where TEnum : SmartEnum<TEnum, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>, IConvertible
    {
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return GetFromName(reader.GetString());

                default:
                    throw new JsonException($"Unexpected token {reader.TokenType} when parsing a smart enum.");
            }
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value.Name);
            }
        }

        private TEnum GetFromName(string name)
        {
            try
            {
                return SmartEnum<TEnum, TValue>.FromName(name);
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error converting value '{name}' to a smart enum.", ex);
            }
        }
    }
}

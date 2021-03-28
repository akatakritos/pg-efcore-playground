using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Demo.Api.Domain;
using MicroElements.Swashbuckle.NodaTime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Demo.Api.Infrastructure.ServiceRegistration
{
    public static class SwaggerConfiguration
    {
        public static void AddAppSwaggerGen(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo.Api", Version = "v1" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.MapType<decimal>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "decimal"
                });

                c.MapType<int>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "int32"
                });

                c.MapType<long>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "int64"
                });

                c.MapType<UnitOfMeasure>(() => new OpenApiSchema
                {
                    Type = "string"
                });

                var jsonSerializerOptions = new JsonSerializerOptions();
                JsonConfiguration.ConfigureSystemTextJson(jsonSerializerOptions);
                c.ConfigureForNodaTimeWithSystemTextJson(jsonSerializerOptions);
            });
        }
    }
}

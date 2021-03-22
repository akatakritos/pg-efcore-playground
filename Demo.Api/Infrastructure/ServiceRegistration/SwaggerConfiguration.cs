using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
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

                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                JsonConfiguration.ConfigureSystemTextJson(jsonSerializerOptions);
                c.ConfigureForNodaTimeWithSystemTextJson(jsonSerializerOptions);
            });
        }
    }
}
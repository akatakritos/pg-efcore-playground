using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Demo.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Demo.Api.Infrastructure.ServiceRegistration
{
    public static class HealthCheckExtensions
    {
        public static void AddAppHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<PlaygroundContext>()
                .AddCheck<CustomHealthCheck>("Custom Check")
                .AddCheck<MigrationHealthCheck>("Migration Status");
        }

        public static void MapAppHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                    await JsonSerializer.SerializeAsync(context.Response.Body,
                        HealthCheckResponse.FromHealthReport(report),
                        new JsonSerializerOptions { WriteIndented = true })
            });
        }
    }

    public class IndividualHealthCheckResponse
    {
        public string Status { get; set; } = null!;
        public string Component { get; set; } = null!;
        public string? Description { get; set; }
        public IReadOnlyDictionary<string, object>? Data { get; set; }
    }

    public class HealthCheckResponse
    {
        public string Status { get; set; } = null!;
        public IEnumerable<IndividualHealthCheckResponse> HealthChecks { get; set; } = null!;
        public double HealthCheckDuration { get; set; }

        public static HealthCheckResponse FromHealthReport(HealthReport report)
        {
            return new()
            {
                Status = report.Status.ToString(),
                HealthChecks = report.Entries.Select(x => new IndividualHealthCheckResponse
                {
                    Component = x.Key,
                    Status = x.Value.Status.ToString(),
                    Description = x.Value.Description,
                    Data = x.Value.Data
                }),
                HealthCheckDuration = report.TotalDuration.TotalMilliseconds
            };
        }
    }
}

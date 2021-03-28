using Demo.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Extensions.Logging;

namespace Demo.Api.Infrastructure.ServiceRegistration
{
    public static class PlaygroundContextRegistration
    {
        public static void AddPlaygroundContext(this IServiceCollection services, IConfiguration configuration,
                                                IHostEnvironment environment)
        {
            services.AddDbContext<PlaygroundContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                        o => o.UseNodaTime())
                    .UseSnakeCaseNamingConvention();

                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.UseLoggerFactory(new SerilogLoggerFactory());
                }
            });
        }
    }
}
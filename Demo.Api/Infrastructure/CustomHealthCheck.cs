using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Demo.Api.Infrastructure
{
    // register in configure services .AddHealthChecks
    public class CustomHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            await Task.Delay(100);
            return HealthCheckResult.Healthy();
        }
    }
}

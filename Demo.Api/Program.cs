using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Demo.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss.ff} {SourceContext,-48} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                // .WriteTo.File(path: "logs/log-.txt",
                //     rollingInterval: RollingInterval.Day,
                //     restrictedToMinimumLevel: LogEventLevel.Debug,
                //     outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {SourceContext,-48} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}
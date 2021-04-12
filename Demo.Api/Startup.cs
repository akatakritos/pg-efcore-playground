using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper;
using Demo.Api.Data;
using Demo.Api.Data.Migrations;
using Demo.Api.Domain;
using Demo.Api.Infrastructure;
using Demo.Api.Infrastructure.Indexing;
using Demo.Api.Infrastructure.ServiceRegistration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Demo.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }

        // Default registration stuff
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddControllers()
                .AddJsonOptions(opt => { JsonConfiguration.ConfigureSystemTextJson(opt.JsonSerializerOptions); });


            services.AddAppSwaggerGen();
            services.AddPlaygroundContext(Configuration, Environment);
            services.AddAppMiniProfiler();

            // TypeDescriptor.AddAttributes(typeof(Instant), new TypeConverterAttribute(typeof(InstantTypeConverter)));

            services.AddAppHealthChecks();
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            // builder.RegisterModule(new MyApplicationModule());

            builder.RegisterModule(new MediatrModule());
            builder.RegisterModule(new ValidationModule());
            builder.RegisterModule(new AutoMapperModule(Environment.IsDevelopment(), typeof(Startup).Assembly));
            builder.RegisterModule(new IndexingModule(Configuration));
            builder.RegisterType<BackgroundMessageDispatcher>().As<IDomainEventDispatcher>().InstancePerLifetimeScope();

            SqlMapper.AddTypeHandler(InstantHandler.Default);
            builder.Register(_ => new PostgresDatabase(Configuration.GetConnectionString("Postgres")))
                .AsImplementedInterfaces();
        }

        // Configure is where you add middleware. This is called after
        // ConfigureContainer. You can use IApplicationBuilder.ApplicationServices
        // here if you need to resolve things from the container.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo.Api v1"));
            }

            app.UseExceptionHandler("/error");
            app.UseHttpsRedirection();
            app.UseMiniProfiler();

            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapAppHealthChecks();
            });

            // If, for some reason, you need a reference to the built container, you
            // can use the convenience extension method GetAutofacRoot.
            // AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            MigrateDatabase();
        }

        private void MigrateDatabase()
        {
            var migrator = new DbUpMigrator(Configuration.GetConnectionString("Postgres"));
            try
            {
                migrator.Migrate();
            }
            catch (DbUpMigrationException ex)
            {
                Log.Logger.Fatal(ex, "Failed at migrating");
                throw;
            }
        }
    }
}

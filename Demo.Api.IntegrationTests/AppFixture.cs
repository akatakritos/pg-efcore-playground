using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Demo.Api.Data;
using Demo.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Respawn;

namespace Demo.Api.IntegrationTests
{
    public static class AppFixture
    {
        private static readonly Checkpoint _checkpoint;
        private static readonly IConfigurationRoot _configuration;
        private static readonly IServiceScopeFactory _scopeFactory;


        static AppFixture()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var startup = new Startup(_configuration, new HostEnvironment());
            var services = new ServiceCollection();

            startup.ConfigureServices(services);
            var builder = new AutofacServiceProviderFactory().CreateBuilder(services);
            startup.ConfigureContainer(builder);
            builder.RegisterType<NullDispatcher>().As<IDomainEventDispatcher>(); // replace with null dispatcher

            var container = builder.Build();

            _scopeFactory = container.Resolve<IServiceScopeFactory>();
            _checkpoint = new Checkpoint
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = new[]
                {
                    "unit_of_measure_lib",
                    "migration_history"
                }
            };

            ConnectionString = _configuration.GetConnectionString("Postgres");
            Debug.Assert(ConnectionString.Contains("_test"));
        }

        public static string ConnectionString { get; }

        public static async Task ResetCheckpoint()
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            await _checkpoint.Reset(conn);
        }

        public static async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<PlaygroundContext>();

                try
                {
                    await dbContext.BeginTransactionAsync().ConfigureAwait(false);

                    await action(scope.ServiceProvider).ConfigureAwait(false);

                    await dbContext.CommitTransactionAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<PlaygroundContext>();

                try
                {
                    await dbContext.BeginTransactionAsync().ConfigureAwait(false);

                    var result = await action(scope.ServiceProvider).ConfigureAwait(false);

                    await dbContext.CommitTransactionAsync().ConfigureAwait(false);

                    return result;
                }
                catch (Exception)
                {
                    dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public static Task ExecuteDbContextAsync(Func<PlaygroundContext, Task> action)
        {
            return ExecuteScopeAsync(sp => action(sp.GetService<PlaygroundContext>()));
        }

        public static Task ExecuteDbContextAsync(Func<PlaygroundContext, IMediator, Task> action)
        {
            return ExecuteScopeAsync(sp => action(sp.GetService<PlaygroundContext>(), sp.GetService<IMediator>()));
        }

        public static Task<T> ExecuteDbContextAsync<T>(Func<PlaygroundContext, Task<T>> action)
        {
            return ExecuteScopeAsync(sp => action(sp.GetService<PlaygroundContext>()));
        }

        public static Task<T> ExecuteDbContextAsync<T>(Func<PlaygroundContext, IMediator, Task<T>> action)
        {
            return ExecuteScopeAsync(sp => action(sp.GetService<PlaygroundContext>(), sp.GetService<IMediator>()));
        }

        public static Task InsertAsync<T>(params T[] entities) where T : class
        {
            return ExecuteDbContextAsync(db =>
            {
                foreach (var entity in entities)
                {
                    db.Set<T>().Add(entity);
                }

                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);

                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
            where TEntity : class
            where TEntity2 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);

                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3 entity3)
            where TEntity : class
            where TEntity2 : class
            where TEntity3 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);
                db.Set<TEntity3>().Add(entity3);

                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(
            TEntity entity, TEntity2 entity2, TEntity3 entity3, TEntity4 entity4)
            where TEntity : class
            where TEntity2 : class
            where TEntity3 : class
            where TEntity4 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);
                db.Set<TEntity3>().Add(entity3);
                db.Set<TEntity4>().Add(entity4);

                return db.SaveChangesAsync();
            });
        }

        public static Task<T> FindAsync<T>(Guid guid)
            where T : class, IModel
        {
            return ExecuteDbContextAsync(db => db.Set<T>().Where(t => t.Key == guid).FirstOrDefaultAsync());
        }

        public static Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        {
            return ExecuteScopeAsync(sp =>
            {
                var mediator = sp.GetService<IMediator>();

                return mediator.Send(request);
            });
        }

        public static Task SendAsync(IRequest request)
        {
            return ExecuteScopeAsync(sp =>
            {
                var mediator = sp.GetService<IMediator>();

                return mediator.Send(request);
            });
        }

        private class HostEnvironment : IHostEnvironment
        {
            public string ApplicationName { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; }
            public string ContentRootPath { get; set; }
            public string EnvironmentName { get; set; } = "Development";
        }
    }
}

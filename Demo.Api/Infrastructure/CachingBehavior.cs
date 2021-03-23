using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Serilog;
using Serilog.Core;

namespace Demo.Api.Infrastructure
{
    /// <summary>
    /// This is not a production implementation of a cache. use a better one with eviction policies
    /// </summary>
    public static class TerribleCache
    {
        private static readonly Dictionary<string, object> _cache = new();

        public static T Get<T>(string key)
        {
            return (T) _cache[key];
        }

        public static void Set<T>(string key, T value)
        {
            _cache[key] = value;
        }

        public static bool HasKey(string key)
        {
            return _cache.ContainsKey(key);
        }

        public static void Remove(string key)
        {
            if (HasKey(key)) _cache.Remove(key);
        }
    }

    public interface ICacheableRequest
    {
        string GetCacheKey();
    }

    public interface ICacheInvalidationRequest
    {
        string GetCacheKeyToInvalidate();
    }

    /// <summary>
    /// Example middleware to do caching. Requests can specify a cache key to be stored under.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        // Log.ForContext<Type> gives a gnarly name due to generics
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger _log = Log.ForContext(Constants.SourceContextPropertyName,
            typeof(CachingBehavior<,>).FullName);

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
                                            RequestHandlerDelegate<TResponse> next)
        {
            // for example, the edit command could invalidate the cache
            if (request is ICacheInvalidationRequest invalidator)
            {
                _log.Information("Removing cache key {CacheKey}", invalidator.GetCacheKeyToInvalidate());
                TerribleCache.Remove(invalidator.GetCacheKeyToInvalidate());
            }

            if (request is ICacheableRequest cacheable)
            {
                var key = cacheable.GetCacheKey();

                if (TerribleCache.HasKey(key))
                {
                    _log.Information("Item {CacheKey} is cached. Handler skipped", key);
                    return TerribleCache.Get<TResponse>(key);
                }

                var result = await next();
                _log.Information("Caching item {CacheKey}", key);
                TerribleCache.Set(key, result);
                return result;
            }

            return await next();
        }
    }
}

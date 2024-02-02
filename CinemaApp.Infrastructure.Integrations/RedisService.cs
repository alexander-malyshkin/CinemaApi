using System.Collections.Concurrent;
using System.Text.Json;
using CinemaApp.Core.ServiceContracts;
using CinemaApp.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CinemaApp.Infrastructure.Integrations
{
    public sealed class RedisService<T> : ICacheService<T>
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly ILogger<RedisService<T>> _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;
        private readonly TimeSpan? _cacheExpiry;
        private readonly IDatabase _redisDb;

        public RedisService(IConnectionMultiplexer connectionMultiplexer, IConfiguration configuration, JsonSerializerOptions jsonSerializerOptions,
                            ILogger<RedisService<T>> logger)
        {
            _redisDb = connectionMultiplexer.GetDatabase();
            _jsonSerializerOptions = jsonSerializerOptions;
            _logger = logger;
            var cacheExpirySeconds = int.TryParse(configuration["CacheOptions:CacheExpirySeconds"], out int resultInt)
                ? resultInt
                : int.MaxValue;
            
            _cacheExpiry = TimeSpan.FromSeconds(cacheExpirySeconds);
            _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        public async Task<T?> GetOrSet(string key, Func<string, CancellationToken, Task<T?>> factory, CancellationToken ct)
        {
            T? foundValue = await Get(key);
            if (foundValue != null)
                return foundValue;
            
            // we use locking below to avoid the situation where two threads are simultaneously trying to make an expensive call to factory.Invoke
            
            // Create a lock object for this key if it doesn't exist yet
            // we are using SemaphoreSlim here because it allows asynchronous waiting
            var keyLock = _locks.GetOrAdd(key, k => new SemaphoreSlim(1));

            // Don't allow more than one thread to run this method at a time by using a static lock object
            using (await keyLock.EnterAsync())
            {
                // check again if the value was set while we were waiting for the lock:
                foundValue = await Get(key);
                if (foundValue is not null)
                    return foundValue;

                ArgumentNullException.ThrowIfNull(factory);

                T? value;

                try
                {
                    value = await factory.Invoke(key, ct);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to get value for populating cache");
                    throw;
                }

                try
                {
                    await SetWhenDoesNotExist(key, value);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to set value in cache");
                    throw;
                }
            }
            
            return await Get(key);
        }


        private async ValueTask<T?> Get(string key)
        {
            RedisValue stringVal = await _redisDb.StringGetAsync(key);
            
            return stringVal != default  
                    ? JsonSerializer.Deserialize<T>(stringVal!, _jsonSerializerOptions)
                    : default(T?);
        }
        
        private async ValueTask SetWhenDoesNotExist(string key, T? value)
        {
            RedisValue stringValue = JsonSerializer.Serialize(value, _jsonSerializerOptions);
            await _redisDb.StringSetAsync(key, stringValue, when: When.NotExists, expiry: _cacheExpiry);
        }
    }
}

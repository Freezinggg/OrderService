using Microsoft.Extensions.Logging;
using OrderService.Application.Interface.Cache;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Cache
{
    public class RedisSharedCounterCache : ISharedCounterCache
    {
        private readonly ILogger<RedisSharedCounterCache> _logger;
        private readonly IDatabase _db;

        public RedisSharedCounterCache(IConnectionMultiplexer redis, ILogger<RedisSharedCounterCache> logger)
        {
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<TimeSpan?> GetTTLAsync(string key)
        {
            return await _db.KeyTimeToLiveAsync(key);
        }

        public async Task<long> IncrementAsync(string key)
        {
            return await _db.StringIncrementAsync(key);
        }

        public async Task SetExpirationAsync(string key, TimeSpan expiration)
        {
            await _db.KeyExpireAsync(key, expiration);
        }
    }
}

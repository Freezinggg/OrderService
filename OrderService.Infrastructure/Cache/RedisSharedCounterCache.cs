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
        private readonly string key = "orderservice:shared-counter";

        public RedisSharedCounterCache(IConnectionMultiplexer redis, ILogger<RedisSharedCounterCache> logger)
        {
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<long> IncrementAsync()
        {
            await _db.StringIncrementAsync(key);
            var val = await _db.StringGetAsync(key);

            return val.HasValue ? (long)val : 0;
        }
    }
}

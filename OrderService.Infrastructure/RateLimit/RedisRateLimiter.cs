using OrderService.Application.Interface;
using OrderService.Application.Interface.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.RateLimit
{
    public class RedisRateLimiter : IRateLimiter
    {
        private readonly ISharedCounterCache _cache;

        public RedisRateLimiter(ISharedCounterCache cache)
        {
            _cache = cache;
        }

        public async Task<RateLimitResult> CheckAsync(string clientId)
        {
            RateLimitResult result = new RateLimitResult();
            string key = $"order_service:rate_limit:{clientId}";
            var count = await _cache.IncrementAsync(key);

            if (count == 1)
            {
                await _cache.SetExpirationAsync(key, TimeSpan.FromSeconds(30));
                result.Allowed = true;
            }

            if(count <= 5) result.Allowed = true;

            result.CurrentCount = count;
            result.RemainingWindow = await _cache.GetTTLAsync(key);

            return result;
        }
    }
}

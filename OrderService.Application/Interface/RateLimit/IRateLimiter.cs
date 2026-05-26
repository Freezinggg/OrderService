using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interface.RateLimit
{
    public interface IRateLimiter
    {
        Task<RateLimitResult> CheckAsync(string clientId);
    }

    public class RateLimitResult
    {
        public bool Allowed { get; set; } = false;

        public long CurrentCount { get; set; } = 0;

        public TimeSpan? RemainingWindow { get; set; }
    }
}

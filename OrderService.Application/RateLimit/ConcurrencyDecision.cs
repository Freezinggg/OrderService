using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.RateLimit
{
    public sealed class ConcurrencyDecision
    {
        public bool IsAllowed { get; }
        public BackoffHint? hint { get;}


        public ConcurrencyDecision(bool isAllowed, BackoffHint? _hint) { 
            IsAllowed = isAllowed;
            hint = _hint;
        }

        public static ConcurrencyDecision Allowed() => new ConcurrencyDecision(true, null);
        public static ConcurrencyDecision Rejected(BackoffHint hint) => new ConcurrencyDecision(false, hint);
    }

    public sealed class BackoffHint
    {
        public int MinDelayMs { get; }
        public int MaxDelayMs { get; }
        public string Strategy { get; }

        private BackoffHint(int minDelayMs, int maxDelayMs, string strategy)
        {
            MinDelayMs = minDelayMs;
            MaxDelayMs = maxDelayMs;
            Strategy = strategy;
        }

        public static BackoffHint ExponentialJitter(
            int minDelayMs = 200,
            int maxDelayMs = 5_000)
            => new(minDelayMs, maxDelayMs, "exponential-jitter");
    }
}

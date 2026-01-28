using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.RateLimit
{
    public class PressureDecision
    {
        public PressureDecision(bool isAllowed, TimeSpan? retryAfter) {
            IsAllowed = isAllowed;
            RetryAfter = retryAfter;
        }

        public bool IsAllowed { get; }
        public TimeSpan? RetryAfter { get; }

        public static PressureDecision Allow() => new(true, null);
        public static PressureDecision Reject(TimeSpan retryAfter) => new(false, retryAfter);
    }
}

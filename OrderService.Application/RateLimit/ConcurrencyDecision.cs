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
        public bool IsRejected => !IsAllowed;

        public ConcurrencyDecision(bool isAllowed) { 
            IsAllowed = isAllowed;
        }

        public static ConcurrencyDecision Allowed() => new ConcurrencyDecision(true);
        public static ConcurrencyDecision Rejected() => new ConcurrencyDecision(false);
    }
}

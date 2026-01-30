using OrderService.Application.RateLimit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interface
{
    public interface IConcurrencyLimiter
    {
        ConcurrencyDecision TryAcquire();
        void Release();
    }
}

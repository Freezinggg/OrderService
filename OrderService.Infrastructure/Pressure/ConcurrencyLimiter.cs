using OrderService.Application.Interface;
using OrderService.Application.RateLimit;
using OrderService.Infrastructure.Observability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Pressure
{
    public sealed class ConcurrencyLimiter : IConcurrencyLimiter
    {
        private readonly IConcurrencyMetric _metric;
        private readonly SemaphoreSlim _semaphore;

        public ConcurrencyLimiter(int capacity, IConcurrencyMetric metric)
        {
            //initialCount = Capacity > how many available right now, maxCount = capacity > max capactiy
            _semaphore = new SemaphoreSlim(capacity, capacity);
            _metric = metric;
        }

        public void Release()
        {
            _metric.Released();
            _semaphore.Release();
        }

        public ConcurrencyDecision TryAcquire()
        {
            //Semaphore.wait() > if available, take capacity, 0 means timeout, which 0 timeout means dont wait. so bbasically saying : if resource free, take it immediately, if not then dont wait. Avoid blocking
            if (!_semaphore.Wait(0))
            {
                _metric.Rejected();
                return ConcurrencyDecision.Rejected();
            }

            _metric.Acquired();
            return ConcurrencyDecision.Allowed();
        }
    }
}

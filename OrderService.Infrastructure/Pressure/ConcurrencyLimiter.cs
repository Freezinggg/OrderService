using OrderService.Application.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Pressure
{
    public sealed class ConcurrencyLimiter : IConcurrencyLimiter
    {
        private readonly SemaphoreSlim _semaphore;

        public ConcurrencyLimiter(int capacity)
        {
            //initialCount = Capacity > how many available right now, maxCount = capacity > max capactiy
            _semaphore = new SemaphoreSlim(capacity, capacity);
        }

        public void Release()
        {
            _semaphore.Release();
        }

        public bool TryAcquire()
        {
            //Semaphore.wait() > if available, take capacity, 0 means timeout, which 0 timeout means dont wait. so bbasically saying : if resource free, take it immediately, if not then dont wait. Avoid blocking
            return _semaphore.Wait(0);
        }
    }
}

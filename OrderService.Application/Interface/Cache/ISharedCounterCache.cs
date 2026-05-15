using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interface.Cache
{
    public interface ISharedCounterCache
    {
        Task<long> IncrementAsync(string key);

        Task SetExpirationAsync(string key, TimeSpan expiration);

        Task<TimeSpan?> GetTTLAsync(string key);
    }
}

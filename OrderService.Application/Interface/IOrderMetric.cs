using OrderService.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interface
{
    public interface IOrderMetric
    {
        void RecordCreateOrder(ResultStatus status, long durationMs);
        void RecordAsyncPendingAge(double seconds);
        void SetPendingCount(long count);
    }
}

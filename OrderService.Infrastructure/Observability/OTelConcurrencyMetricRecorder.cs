using OrderService.Application.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Observability
{
    public sealed  class OTelConcurrencyMetricRecorder : IConcurrencyMetric
    {
        private static readonly Meter Meter = new("OrderService.Concurrency");

        private static readonly UpDownCounter<long> ConcurrencyCurrent =
            Meter.CreateUpDownCounter<long>("concurrency_current");

        private static readonly Counter<long> ConcurrencyRejected =
            Meter.CreateCounter<long>("concurrency_rejected_total");

        public void Acquired() => ConcurrencyCurrent.Add(1);
        public void Released() => ConcurrencyCurrent.Add(-1);
        public void Rejected() => ConcurrencyRejected.Add(1);
    }
}

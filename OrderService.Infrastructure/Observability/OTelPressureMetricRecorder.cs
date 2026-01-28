using OrderService.Application.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Observability
{
    public sealed class OTelPressureMetricRecorder : IPressureMetric
    {
        private static readonly Meter Meter = new("OrderService.Pressure");

        private static readonly Counter<long> Allowed =
            Meter.CreateCounter<long>("pressure_allowed_total");

        private static readonly Counter<long> Rejected =
            Meter.CreateCounter<long>("pressure_rejected_total");

        public void RecordAllowed() => Allowed.Add(1);
        public void RecordRejected() => Rejected.Add(1);
    }
}

using OrderService.Application.Common;
using OrderService.Application.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Observability
{
    public class OTelOrderMetricRecorder : IOrderMetric
    {
        private long _currentPendingCount;
        private long _processingCurrent;


        private static readonly Meter Meter = new("OrderService.Metrics");

        //Request
        private static readonly Counter<long> RequestSucceededCounter =
            Meter.CreateCounter<long>("order_create_success_total");

        private static readonly Counter<long> RequestInvalidCounter =
            Meter.CreateCounter<long>("order_create_invalid_total");

        private static readonly Counter<long> RequestFailCounter =
            Meter.CreateCounter<long>("order_create_business_reject_total");

        private static readonly Counter<long> RequestServiceUnavailable =
            Meter.CreateCounter<long>("order_create_infra_fail_total");

        private static readonly Counter<long> RequestError =
            Meter.CreateCounter<long>("order_create_error_total");

        private static readonly Histogram<long> RequestDurationMs =
            Meter.CreateHistogram<long>("order_create_duration_ms", "ms");

        //Order life-line timeline
        private static readonly Histogram<double> OrderPendingAge =
            Meter.CreateHistogram<double>(
                "orders_completion_age_seconds",
                unit: "seconds",
                description: "Time an order spent pending before completion");

        private static readonly Counter<long> OrderCompletedTotal =
            Meter.CreateCounter<long>("orders_completed_total");

        private static readonly Counter<long> OrderProcessingExpiredTotal =
             Meter.CreateCounter<long>("orders_processing_expired_total");

        public OTelOrderMetricRecorder()
        {
            Meter.CreateObservableGauge<long>(
                "orders_pending_count",
                () => new Measurement<long>(_currentPendingCount),
                description: "Current number of orders in Pending state");

            Meter.CreateObservableGauge<long>(
            "orders_processing_current_count",
            () => new Measurement<long>(_processingCurrent),
            description: "Current number of orders in Processing state");
        }


        public void RecordAsyncPendingAge(double seconds) => OrderPendingAge.Record(seconds);

        public void RecordCreateOrder(ResultStatus status, long durationMs)
        {
            switch (status)
            {
                case ResultStatus.Success:
                    RequestSucceededCounter.Add(1);
                    break;
                case ResultStatus.Invalid:
                    RequestInvalidCounter.Add(1);
                    break;
                case ResultStatus.Fail:
                    RequestFailCounter.Add(1);
                    break;
                case ResultStatus.ServiceUnavailable:
                    RequestServiceUnavailable.Add(1);
                    break;
                case ResultStatus.Error:
                    RequestError.Add(1);
                    break;
            }

            RequestDurationMs.Record(durationMs);
        }

        public void SetPendingCount(long count) => _currentPendingCount = count;

        public void RecordCompleted() => OrderCompletedTotal.Add(1);

        public void RecordProcessingExpired(long count) => OrderProcessingExpiredTotal.Add(count);

        public void SetProcessingCurrent(long value) => _processingCurrent = value;
    }
}

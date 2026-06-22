using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Configuration.Kafka
{
    public sealed class KafkaOptions
    {
        public string BootstrapServers { get; set; } = string.Empty;

        public string OrderEventsTopic { get; set; } = string.Empty;

        public string OrderProjectionGroup { get; set; } = string.Empty;
    }
}

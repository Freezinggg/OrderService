using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.RateLimit
{
    public class PressureContext
    {
        public PressureContext(string endpoint, Guid? customerId, string ipAddress) {
            Endpoint = endpoint;
            CustomerId = customerId;
            IpAddress = ipAddress;        
        }

        public string Endpoint { get; }
        public Guid? CustomerId { get; }
        public string IpAddress { get; }
    }
}

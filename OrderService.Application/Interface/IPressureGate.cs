using OrderService.Application.RateLimit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interface
{
    public interface IPressureGate
    {
        PressureDecision Evaluate(PressureContext context);
    }
}

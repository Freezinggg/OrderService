using OrderService.Application.Interface;
using OrderService.Application.RateLimit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Pressure
{
    public class PressureGate : IPressureGate
    {
        const int Limit = 2;
        TimeSpan Window = TimeSpan.FromSeconds(10);

        private static readonly Dictionary<(string Endpoint, string Ip), WindowEntry> _windows = new();

        public PressureDecision Evaluate(PressureContext context)
        {
            var key = (context.Endpoint, context.IpAddress);
            DateTime now = DateTime.Now;

            //Check if endpoint + ip exist. if not exist -> create new entry, else return true + entry
            if (!_windows.TryGetValue(key, out var entry))
            {
                _windows[key] = new WindowEntry() { WindowStart = now, Count = 1 };
                return PressureDecision.Allow();
            }

            //If endpoint + ip exist, check entry. if now - windowstart (time where entry was added) > window is true (means its alrerady past Window timespan which configured on variable), then reset
            if(now - entry.WindowStart > Window)
            {
                _windows[key] = new WindowEntry
                {
                    WindowStart = now,
                    Count = 1
                };

                return PressureDecision.Allow();
            }

            //If endpoint + ip exist, but check entry (now-windowstart is less than Window) it means client/user trying to hit same endpoint+ip, add entry.Count by 1
            entry.Count++;

            //Check entry limit after every count, if hit limit, set retryafter
            if(entry.Count > Limit)
            {
                TimeSpan retryAfter = (entry.WindowStart + Window) - now; //ex: WindowStart = 14:00:10, Window Endtime (cannot make request if now < endtime) is 14:00:20, now is 14:00:15, then we need 5s until it unlocks/can try again/reset
                return PressureDecision.Reject(retryAfter);
            }

            return PressureDecision.Allow();
        }

        private sealed class WindowEntry
        {
            public DateTime WindowStart;
            public int Count;
        }
    }

}

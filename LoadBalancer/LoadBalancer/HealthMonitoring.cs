using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    class HealthMonitoring
    {
        public enum ServerStatus
        {
            Good,
            Busy,
            Offline,
            Slow,
            Unknown
        }

    }
}

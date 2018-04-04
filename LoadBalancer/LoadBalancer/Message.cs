using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    class Message
    {
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }

        public Message()
        {
            Headers = new Dictionary<string, string>();
        }
    }
}

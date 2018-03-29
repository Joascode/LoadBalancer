using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    class Server
    {
        Dictionary<int, Client> clients = new Dictionary<int, Client>();
    }
}

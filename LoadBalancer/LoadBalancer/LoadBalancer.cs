using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    class LoadBalancer
    {
        Dictionary<int, Server> servers = new Dictionary<int, Server>();
        Dictionary<int, int> sessions = new Dictionary<int, int>();

        TcpListener tcpListener;

        public LoadBalancer()
        {

        }

        public void Listen()
        {
            //Start listener.

            //Check on TcpClientsAcceptance > server or client.

            //Pass client to Server for handling messages and callback to forward response to client.
        }

    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    class LoadBalancer
    {
        Dictionary<int, Server> servers = new Dictionary<int, Server>();
        Dictionary<int, int> sessions = new Dictionary<int, int>();
        Dictionary<int, ClientChatter> clients = new Dictionary<int, ClientChatter>();

        TcpListener tcpListener;

        public int BufferSize { get; set; }

        public LoadBalancer(string ip = "127.0.0.1", int port = 8080)
        {
            // Encapsulate with try/catch
            tcpListener = new TcpListener(IPAddress.Parse(ip), port);
        }

        public async void Listen()
        {
            //Start listener.
            tcpListener.Start();

            try
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                if(client != null)
                {
                    //Task.Run(() => HandleClientConnection(client));
                    ClientChatter chatter = new ClientChatter(client, ClientMessageReceivedCallback);
                    clients.Add(ClientChatter.Id, chatter);
                }
            }
            catch(Exception e) when (
                e is InvalidOperationException ||
                e is SocketException ||
                e is ArgumentNullException ||
                e is ArgumentException
            )
            {
                Console.WriteLine("An error occured: " + e.Message);
            }
        }

        public void AddServer(string ip, int port)
        {
            Server server = new Server(ip, port, ServerMessageReceivedCallback);
            servers.Add(Server.Id, server);
        }

        // Bridge between Server and Client.
        public void ServerMessageReceivedCallback(byte[] message)
        {
            string stringMessage = Encoding.ASCII.GetString(message);
            Client client = JsonConvert.DeserializeObject<Client>(stringMessage);

            if(clients.TryGetValue(client.Id, out ClientChatter chatter))
            {
                chatter.AddMessage(client);
            }
        }

        // Bridge between Client and Server.
        public void ClientMessageReceivedCallback(byte[] message)
        {
            string stringMessage = Encoding.ASCII.GetString(message);
            Client client = JsonConvert.DeserializeObject<Client>(stringMessage);

            if (sessions.TryGetValue(client.Id, out int serverId))
            {
                if(servers.TryGetValue(serverId, out Server server))
                {
                    server.AddMessage(client);
                }
                
            }
            else
            {
                Random random = new Random();
                int rndNumber = random.Next(servers.Count);

                Server server = servers.ElementAt(rndNumber).Value;
                server.AddClient(client);
            }
        }

    }
}

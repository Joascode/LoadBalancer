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
    class LoadBalancerImpl
    {
        Dictionary<string, Server> servers = new Dictionary<string, Server>();
        SessionStorage sessions;
        Dictionary<string, ClientChatter> clients = new Dictionary<string, ClientChatter>();

        TcpListener tcpListener;

        public int BufferSize { get; set; }

        public LoadBalancerImpl(string ip = "127.0.0.1", int port = 8080)
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
                    Console.WriteLine("Client connected.");
                    //Task.Run(() => HandleClientConnection(client));
                    ClientChatter chatter = new ClientChatter(client, ClientMessageReceivedCallback);
                    clients.Add(chatter.Id, chatter);
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
            servers.Add(server.Id, server);
        }

        // Bridge between Server and Client.
        public void ServerMessageReceivedCallback(byte[] message)
        {
            string stringMessage = Encoding.ASCII.GetString(message);
            Console.WriteLine(stringMessage);
            Message client = JsonConvert.DeserializeObject<Message>(stringMessage);

            if(clients.TryGetValue(client.Headers["Id"], out ClientChatter chatter))
            {
                chatter.AddMessage(client);
            }
        }

        // Bridge between Client and Server.
        public void ClientMessageReceivedCallback(Message client)
        {

            if(sessions != null)
            {
                if (client.Headers.ContainsKey("Id"))
                {
                    if (sessions.GetServerIdForClient(client.Headers["Id"], out string serverId))
                    {
                        Console.WriteLine("Session exists.");
                        if (servers.TryGetValue(serverId, out Server server))
                        {
                            server.AddMessage(client);
                        }

                    }
                }
            }
            
            else
            {
                Console.WriteLine("Connecting to new Server.");
                Random random = new Random();
                int rndNumber = random.Next(servers.Count);
                Console.WriteLine($"Server #{rndNumber}");
                Server server = servers.ElementAt(rndNumber).Value;

                client.Headers.Add("Id", random.Next().ToString());
                Console.WriteLine(client.Headers);

                server.AddClient(client);
                sessions.AddSession(client.Headers["Id"], server.Id);
            }
        }
    }
}

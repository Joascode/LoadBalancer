using LBAlgorithm;
using Messages;
using Newtonsoft.Json;
using ServerAffinity;
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
        public IServerAffinity<string, string> sessions { get; set; }  // = new SessionStorage();
        Dictionary<string, ClientChatter> clients = new Dictionary<string, ClientChatter>();
        public ILBAlgorithm algorithm { get; set; }
        TcpListener tcpListener;

        public int BufferSize { get; set; }
        private const string IP_ADDRESS = "127.0.0.1";
        private const int PORT = 8080;
        private bool listening = false;

        public LoadBalancerImpl(string ip = IP_ADDRESS, int port = PORT)
        {
            // Encapsulate with try/catch
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse(ip), port);
            }
            catch(Exception e) when (
                e is ArgumentNullException ||
                e is ArgumentOutOfRangeException
            )
            {
                Console.WriteLine("Couldn't create a TcpListener, reason: " + e.Message);
            }

            
            //AlgorithmPicker("RD");
        }

        /*public void AlgorithmPicker(string algoName)
        {
            algorithm = AlgorithmFactory.GetAlgorithm(algoName);
        }*/

        public async void Listen()
        {
            listening = true;
            tcpListener.Start();

            while(listening)
            {
                try
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync();
                    if (client != null)
                    {
                        Console.WriteLine("Client connected.");
                        ClientChatter chatter = new ClientChatter(client, ClientMessageReceivedCallback);
                        clients.Add(chatter.Id, chatter);
                    }
                }
                catch (Exception e) when (
                    e is InvalidOperationException ||
                    e is SocketException ||
                    e is ArgumentNullException ||
                    e is ArgumentException
                )
                {
                    Console.WriteLine("Something went wrong when accepting a TcpClient, reason: " + e.Message);
                }
            }
        }

        public void AddServer(string ip, int port, Action<Server> AddServerCallback)
        {
            Server server = new Server(ip, port, ServerMessageReceivedCallback);
            servers.Add(server.Id, server);
            AddServerCallback(server);
        }

        // Bridge between Server and Client.
        public void ServerMessageReceivedCallback(Message<string, string> client)
        {
            if(clients.TryGetValue(client.Headers["Id"], out ClientChatter chatter))
            {
                chatter.AddMessage(client);
            }
        }

        // Bridge between Client and Server.
        public void ClientMessageReceivedCallback(Message<string, string> client)
        {
            //TODO: Clean this up in the Sessionstorage implementation. Unnecessary information here. See new function at the bottom.
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
            
            //TODO: Clean this shit up.
            else
            {
                Console.WriteLine("Connecting to new Server.");
                Random random = new Random();

                Server server = ServerPicker();

                client.Headers.Add("Id", random.Next().ToString());

                server.AddClient(client);
            }
        }

        //Add session check.
        private Server ServerPicker()
        {
            int pos = algorithm.GetServerArrayPosition(servers.Count);
            Console.WriteLine("Chosen Server: " + pos);
            return servers.ElementAt(pos).Value;
        }
    }
}

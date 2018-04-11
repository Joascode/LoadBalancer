using LBAlgorithm;
using Messages;
using Newtonsoft.Json;
using ServerAffinity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        public IServerAffinity Sessions { get; set; }  // = new SessionStorage();
        Dictionary<string, ClientChatter> clients = new Dictionary<string, ClientChatter>();
        public ILBAlgorithm Algorithm { get; set; }
        TcpListener tcpListener;
        Stopwatch stopWatch;

        public int BufferSize { get; set; }
        private const string IP_ADDRESS = "127.0.0.1";
        private const int PORT = 8080;
        public bool Listening = false;

        public LoadBalancerImpl(string ip = IP_ADDRESS, int port = PORT)
        {
            // Encapsulate with try/catch
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse(ip), port);
                stopWatch = new Stopwatch();
            }
            catch(Exception e) when (
                e is ArgumentNullException ||
                e is ArgumentOutOfRangeException
            )
            {
                Console.WriteLine("Couldn't create a TcpListener, reason: " + e.Message);
            }
        }

        public async void Listen()
        {
            //Add closing off listening in the UI.
            Listening = true;
            tcpListener.Start();
            stopWatch.Start();

            while(Listening)
            {
                try
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync();
                    if (client != null)
                    {
                        Console.WriteLine("Client connected.");
                        ClientChatter chatter = new ClientChatter(client, SendMessageToServer);
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

                //CalculateServersLatency(5000);
            }
        }

        public void AddServer(string ip, int port, Action<Server> AddServerCallback)
        {
            Server server = new Server(ip, port, SendMessageToClient);
            servers.Add(server.Id, server);
            AddServerCallback(server);
        }

        // Bridge between Server and Client.
        public void SendMessageToClient(Message<string, string> client)
        {
            if(clients.TryGetValue(client.Headers["Id"], out ClientChatter chatter))
            {
                chatter.AddMessage(client);
            }
        }

        // Bridge between Client and Server.
        public void SendMessageToServer(Message<string, string> client)
        {
            if (ServerAffinityExists(client, out Server server) || AlgorithmServerPicker(client, out server))
            {
                server.AddHeader("Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), client);
                server.AddMessage(client);
                server.MessageCounter++;
                Console.WriteLine(server.MessageCounter);
            }
        }

        //TODO: Fix this affinity shit to work with sessions or cookies.
        private bool ServerAffinityExists(Message<string, string> message, out Server server)
        {
            if (Sessions != null)
            {
                if (message.Headers.ContainsKey("Id"))
                {
                    if (Sessions.GetServerIdForClient(message, out string serverId))
                    {
                        Console.WriteLine("Session exists.");
                        if (servers.TryGetValue(serverId, out Server sessionServer))
                        {
                            server = sessionServer;
                            return true;
                        }

                    }
                }
            }
            Console.WriteLine("No server affinity set.");
            server = null;
            return false;
        }

        //Add session check.
        private bool AlgorithmServerPicker(Message<string, string> message, out Server serverOut)
        {
            if(Algorithm != null)
            {
                int pos = Algorithm.GetServerArrayPosition(servers.Count);
                Server server = servers.ElementAt(pos).Value;

                Console.WriteLine("Chosen Server: " + pos);
                Console.WriteLine("Connecting to new Server.");

                //server.AddClient(message);
                serverOut = server;
                return true;
            }
            else
            {
                Console.WriteLine("No algorithm selected.");
                message.Body = "500: Internal Server Error.";
                SendMessageToClient(message);
                serverOut = null;
                return false;
            }
            
        }

        public void CalculateServersLatency(int delay, Action action)
        {
            while(Listening)
            {
                //Console.WriteLine("Calculating Latency's.");
                if (stopWatch.ElapsedMilliseconds > delay)
                {
                    foreach (Server server in servers.Values)
                    {
                        server.CalculateLatency();
                        server.CalculateMessagePerSecond(delay);
                    }
                    stopWatch.Restart();
                    action();
                }
            } 
        }
    }
}

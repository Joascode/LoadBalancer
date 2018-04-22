using LBAlgorithm;
using Messages;
using Newtonsoft.Json;
using ServerAffinity;
using ServerChatters;
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
        Dictionary<string, ServerChatter> servers = new Dictionary<string, ServerChatter>();
        public IServerAffinity Sessions { get; set; }
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

        //Handle all the clean up.
        public void Stop()
        {
            foreach(ServerChatter server in servers.Values)
            {
                server.CloseConnection();
            }
            foreach(ClientChatter client in clients.Values)
            {
                client.CloseConnection();
            }

            tcpListener.Stop();
            stopWatch.Stop();
            Listening = false;
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
                        ClientChatter chatter = new ClientChatter(client, SendMessageToServer, timeout: 5000);
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

        public void AddServer(string ip, int port, Action<ServerChatter> AddServerCallback)
        {
            foreach(ServerChatter sc in servers.Values)
            {
                if(sc.Port == port)
                {
                    Console.WriteLine("Port is already in use.");
                    return;
                }
            }

            ServerChatter server = new ServerChatter(ip, port, SendMessageToClient);
            servers.Add(server.Id, server);
            AddServerCallback(server);

            Console.WriteLine($"Added server with port: {port}");
        }

        public void ReconnectServer(string serverId)
        {
            if (servers.TryGetValue(serverId, out ServerChatter server))
            {
                Console.WriteLine("Trying to reconnect to server.");
                Task.Run(() => server.ReconnectServer());
            }
        }

        public void RemoveServer(string serverId, Action<ServerChatter> RemoveServerCallback)
        {
            if(servers.TryGetValue(serverId, out ServerChatter server))
            {
                server.CloseConnection();
                servers.Remove(serverId);
                RemoveServerCallback(server);
            }
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
            //Fix this to first check if ServerAffinity returns a server or not.
            if (ServerAffinityExists(client, out ServerChatter server) || AlgorithmServerPicker(client, out server))
            {
                if(server == null)
                {
                    client.Body = "503: Internal Server Error.";
                    SendMessageToClient(client);
                }
                else if(server.Status == ServerChatter.ServerStatus.Offline)
                {
                    client.Body = "503: Internal Server Error.";
                    SendMessageToClient(client);
                }
                else
                {
                    server.UpdateHeader("Timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(), client);
                    server.AddMessage(client);
                    server.MessageCounter++;
                    Console.WriteLine(server.MessageCounter);
                }
            }
            else if (server == null)
            {
                SendMessageToClient(client);
            }
        }

        //TODO: Fix this affinity shit to work with sessions or cookies.
        private bool ServerAffinityExists(Message<string, string> message, out ServerChatter server)
        {
            if (Sessions != null)
            {
                if (message.Headers.ContainsKey("Id"))
                {
                    if (Sessions.GetServerIdForClient(message, out string serverId))
                    {
                        Console.WriteLine("Session exists.");
                        if (servers.TryGetValue(serverId, out ServerChatter sessionServer))
                        {
                            server = sessionServer;
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Server doesn't exist where it used to.");
                            server = null;
                            return true;
                        }
                    }
                    else
                    {
                        if(AlgorithmServerPicker(message, out ServerChatter randomServer))
                        {
                            Console.WriteLine("Adding client to session.");
                            Sessions.AddSession(message, randomServer.Id);
                            server = randomServer;
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Couldn't set server affinity.");
                            server = null;
                            return false;
                        }
                    }
                }
            }
            Console.WriteLine("No server affinity set.");
            server = null;
            return false;
        }

        //Add session check.
        private bool AlgorithmServerPicker(Message<string, string> message, out ServerChatter serverOut)
        {
            if(Algorithm != null)
            {
                ServerChatter server = Algorithm.GetServerArrayPosition(servers);
                if(server == null)
                {
                    Console.WriteLine("No server found. Are you sure servers are listening?");
                    message.Body = "503: No Server Found.";
                    serverOut = server;
                    return false;
                }
                Console.WriteLine("Chosen Server: " + server.Id);
                Console.WriteLine("Connecting to new Server.");

                //server.AddClient(message);
                serverOut = server;
                return true;
            }
            else
            {
                Console.WriteLine("No algorithm selected.");
                message.Body = "500: Internal Server Error.";
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
                    foreach (ServerChatter server in servers.Values)
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

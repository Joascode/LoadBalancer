//using Chatter;
//using Messages;
//using System;
//using System.Diagnostics;
//using System.Net.Sockets;
//using System.Threading.Tasks;

//namespace ServerChatters
//{
//    public class ServerChatter : Chatter<string, string>
//    {
//        private const int BUFFER_SIZE = 1024;
//        public long Latency { get; set; }

//        private long TimeTracker;
//        public int MessageCounter;
//        public int AverageNrMessages { get; set; }
//        public ServerStatus Status { get; set; }
//        private bool pinging;

//        public enum ServerStatus
//        {
//            Good,
//            Busy,
//            Offline,
//            Reconnecting,
//            Slow,
//            Unknown
//        }

//        public ServerChatter(string ip, int port, Action<Message<string, string>> callback, int timeout = 2000) : base(ip, port, callback, timeout)
//        {
//            Random random = new Random();
//            Id = random.Next(1, 200).ToString();
//            Console.WriteLine($"ServerId: {Id}");
//            Status = ServerStatus.Unknown;
//        }

//        public void AddClient(Message<string, string> client)
//        {
//            if (client.Headers.TryGetValue("Id", out string Id))
//            {
//                Console.WriteLine(client.Body);
//                AddMessage(client);
//            }
//        }

//        public void ReconnectServer()
//        {
//            if (Status == ServerStatus.Offline)
//            {
//                Stopwatch stopwatch = new Stopwatch();
//                stopwatch.Start();
//                pinging = true;
//                int repingTime = 5000;
//                int maxPingCount = 5;
//                int currentPingCount = 0;

//                SetServerStatus();

//                while (pinging)
//                {
//                    if (stopwatch.ElapsedMilliseconds > repingTime)
//                    {
//                        Console.WriteLine("Pinging");
//                        if (TryConnect(Port, pinging))
//                        {
//                            Console.WriteLine($"Reconnected to server #{Id} on port: {Port}");
//                            SetServerStatus();
//                            pinging = false;
//                        }
//                        stopwatch.Restart();
//                        currentPingCount++;

//                        if (currentPingCount == maxPingCount)
//                        {
//                            Console.WriteLine("Could not connect to server.");
//                            SetServerStatus();
//                            return;
//                        }
//                    }
//                }
//            }
//        }

//        private bool TryConnect(int port, bool pinging)
//        {
//            try
//            {
//                tcpClient = new TcpClient();
//                tcpClient.Connect("127.0.0.1", port);
//                Task.Run(() => HandleMessages());
//                connected = true;
//                return true;
//            }
//            catch (Exception e) when (
//                e is ArgumentException ||
//                e is ArgumentOutOfRangeException ||
//                e is SocketException ||
//                e is ObjectDisposedException
//            )
//            {
//                if (!pinging)
//                {
//                    Console.WriteLine("Failed to connect to server.");
//                    Console.WriteLine("Reason: " + e.Message);
//                }
//                return false;
//            }
//        }

//        public void CreateHeader(string headerName, string headerValue, Message<string, string> message)
//        {
//            if (!message.Headers.ContainsKey(headerName))
//            {
//                message.Headers.Add(headerName, headerValue);
//            }
//        }

//        public string GetHeaderValue(string headerName, Message<string, string> message)
//        {
//            if (message.Headers.TryGetValue(headerName, out string value))
//            {
//                return value;
//            }
//            else
//            {
//                return null;
//            }
//        }

//        public void UpdateHeader(string headerName, string headerValue, Message<string, string> message)
//        {
//            if (message.Headers.ContainsKey(headerName))
//            {
//                message.Headers[headerName] = headerValue;
//            }
//            else
//            {
//                message.Headers.Add(headerName, headerValue);
//            }
//        }

//        public void DeleteHeader(string headerName, Message<string, string> message)
//        {
//            if (message.Headers.ContainsKey(headerName))
//            {
//                message.Headers.Remove(headerName);
//            }
//        }

//        public void CalculateLatency()
//        {
//            if (MessageCounter != 0 && TimeTracker != 0)
//            {
//                Latency = TimeTracker / MessageCounter;
//                Console.WriteLine("Calculating Latency.");
//            }
//        }

//        public void CalculateMessageLatency(Message<string, string> message)
//        {
//            if (message.Headers.TryGetValue("Timestamp", out string msgTimestamp))
//            {
//                if (long.TryParse(msgTimestamp, out long timestamp))
//                {
//                    TimeTracker += DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;
//                }
//            }
//        }

//        public void CalculateMessagePerSecond(int delayInMilliSeconds)
//        {
//            if (delayInMilliSeconds != 0 && MessageCounter != 0)
//            {
//                AverageNrMessages = MessageCounter / (delayInMilliSeconds / 1000);

//                MessageCounter = 0;
//                TimeTracker = 0;

//            }
//            SetServerStatus();
//        }

//        private void SetServerStatus()
//        {
//            if (connected == false && pinging == true)
//            {
//                Status = ServerStatus.Reconnecting;
//            }
//            else if (connected == false)
//            {
//                Status = ServerStatus.Offline;
//            }
//            else if (Latency > 500)
//            {
//                Status = ServerStatus.Slow;
//            }
//            else if (MessageCounter > 3)
//            {
//                Status = ServerStatus.Busy;
//            }
//            else
//            {
//                Status = ServerStatus.Good;
//            }
//        }

//        public override void SetMessageIdHeader(Message<string, string> message)
//        {
//            if (!message.Headers.ContainsKey("Id"))
//            {
//                Random random = new Random();
//                message.Headers.Add("Id", random.Next().ToString());
//            }
//        }

//        public override void HijackReadMessage(Message<string, string> message)
//        {
//            message.Body = $"Hello from Server #{Id}";
//            CalculateMessageLatency(message);
//        }

//        public override void DisconnectedEvent()
//        {
//            connected = false;
//        }
//    }
//}

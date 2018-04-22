//using Messages;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace Chatter
//{
//    public abstract class Chatter<K, V>
//    {
//        protected Queue<Message<K, V>> messages = new Queue<Message<K, V>>();
//        protected Action<Message<K, V>> callback;

//        protected TcpClient tcpClient;

//        public string Id { get; set; }
//        public int Port { get; set; }

//        private const int BUFFER_SIZE = 1024;
//        private int timeout;
//        protected bool connected = false;

//        public Chatter(string ip, int port, Action<Message<K, V>> callback, int timeout = 2000)
//        {
//            this.callback = callback;
//            Port = port;
//            this.timeout = timeout;

//            tcpClient = new TcpClient();
//            try
//            {
//                tcpClient.Connect(ip, port);
//                RunMessageTask();
//            }
//            catch (Exception e) when (
//                e is ArgumentNullException ||
//                e is ArgumentOutOfRangeException ||
//                e is SocketException ||
//                e is ObjectDisposedException
//            )
//            {
//                Console.WriteLine("Could not connect to server: " + e.Message);
//            }
//        }

//        public Chatter(TcpClient client, Action<Message<K, V>> callback, int timeout = 2000)
//        {
//            this.timeout = timeout;
//            tcpClient = client;
//            this.callback = callback;
//            RunMessageTask();
//        }

//        public void CloseConnection()
//        {
//            connected = false;
//            tcpClient.Dispose();
//            tcpClient.Close();
//            Console.WriteLine("Closed connections.");
//        }

//        public void AddMessage(Message<K, V> client)
//        {
//            messages.Enqueue(client);
//        }

//        protected void RunMessageTask()
//        {
//            try
//            {
//                Task.Run(() => HandleMessages());
//            }
//            catch (Exception e) when (
//                e is ArgumentNullException
//            )
//            {
//                Console.WriteLine("An error occured in the server: " + e.Message);
//            }
//        }

//        //TODO: Clean this shit up.
//        protected void HandleMessages()
//        {
//            Stopwatch stopwatch = new Stopwatch();
//            stopwatch.Start();
//            connected = true;
//            int bytesRead;
//            byte[] buffer = new byte[BUFFER_SIZE];

//            using (NetworkStream stream = tcpClient.GetStream())
//            using (MemoryStream ms = new MemoryStream())
//            {
//                while (connected)
//                {
//                    if (HealthPing(stopwatch))
//                    {
//                        break;
//                    }
//                    if (tcpClient.Connected)
//                    {
//                        if (messages.Count > 0)
//                        {
//                            Console.WriteLine("Writing.");
//                            Message<K, V> messageDequeued = messages.Dequeue();

//                            string clientAsString = JsonConvert.SerializeObject(messageDequeued);
//                            try
//                            {
//                                stream.WriteAsync(Encoding.ASCII.GetBytes(clientAsString), 0, clientAsString.Length);
//                            }
//                            catch (Exception e)
//                            {
//                                Console.WriteLine("Server disconnected.");
//                                DisconnectedEvent();
//                                connected = false;
//                                messageDequeued.Body = "404: Server not Found.";
//                                callback(messageDequeued);
//                                return;
//                            }
//                        }

//                        if (stream.DataAvailable)
//                        {
//                            do
//                            {
//                                try
//                                {
//                                    bytesRead = stream.Read(buffer, 0, buffer.Length);
//                                    Console.WriteLine("Reading.");
//                                }
//                                catch (IOException e)
//                                {
//                                    Console.WriteLine("Server disconnected.");
//                                    DisconnectedEvent();
//                                    connected = false;
//                                    return;
//                                }
//                                ms.Write(buffer, 0, bytesRead);
//                            } while (stream.DataAvailable);

//                            Message<K, V> message = ConvertByteToMessage(ms.ToArray());

//                            HijackReadMessage(message);

//                            callback(message);
//                            ms.SetLength(0);

//                            buffer = new byte[BUFFER_SIZE];
//                            bytesRead = 0;
//                        }

//                    }
//                }
//            }

//            Console.WriteLine("Shutting down.");
//        }

//        public abstract void SetMessageIdHeader(Message<K, V> message);

//        public abstract void HijackReadMessage(Message<K, V> message);

//        public abstract void DisconnectedEvent();

//        private Message<K, V> ConvertByteToMessage(byte[] message)
//        {
//            string stringMessage = Encoding.ASCII.GetString(message);
//            Message<K, V> client = JsonConvert.DeserializeObject<Message<K, V>>(stringMessage);
//            SetMessageIdHeader(client);
//            Console.WriteLine(JsonConvert.SerializeObject(client));
//            return client;
//        }

//        private bool HealthPing(Stopwatch stopwatch)
//        {
//            if (stopwatch.ElapsedMilliseconds > timeout)
//            {
//                Console.WriteLine("Health ping.");
//                try
//                {
//                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
//                    {
//                        Console.WriteLine("Pinging.");
//                        byte[] buff = new byte[1];
//                        if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
//                        {
//                            Console.WriteLine("Disconnected.");
//                            connected = false;
//                            stopwatch.Stop();
//                            // Client disconnected

//                            return true;
//                        }
//                    }
//                }
//                catch (Exception e) when (
//                    e is SocketException ||
//                    e is ObjectDisposedException
//                )
//                {
//                    Console.WriteLine("Disconnected.");
//                    connected = false;
//                    stopwatch.Stop();
//                    return true;
//                }
//                stopwatch.Restart();
//            }
//            return false;
//        }
//    }
//}

using Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    abstract class Chatter<T,V>
    {
        //Dictionary<string, Message<string, string>> clients = new Dictionary<string, Message<string, string>>();
        protected Queue<Message<T, V>> messages = new Queue<Message<T, V>>();
        protected Action<Message<T, V>> callback;

        protected TcpClient tcpClient;

        public string guid = Guid.NewGuid().ToString();

        public string Id { get; set; }

        private const int BUFFER_SIZE = 1024;
        protected bool connected = false;

        public Chatter(string ip, int port, Action<Message<T, V>> callback)
        {
            Random random = new Random();
            int randomId = random.Next(1, 10);
            Id = randomId.ToString();

            tcpClient = new TcpClient();
            tcpClient.Connect(ip, port);

            this.callback = callback;
            RunMessageTask();
        }

        public Chatter(TcpClient client, Action<Message<T, V>> callback)
        {
            Random random = new Random();
            int randomId = random.Next(1, 10);
            Id = randomId.ToString();
            tcpClient = client;
            this.callback = callback;
            RunMessageTask();
        }

        public void AddMessage(Message<T, V> client)
        {
            messages.Enqueue(client);
        }

        protected void RunMessageTask()
        {
            try
            {
                Task.Run(() => HandleMessages());
            }
            catch (Exception e) when (
                e is ArgumentNullException
            )
            {
                Console.WriteLine("An error occured in the server: " + e.Message);
            }
        }

        //TODO: Clean this method up.
        private void HandleMessages()
        {
            connected = true;
            int bytesRead;
            byte[] buffer = new byte[BUFFER_SIZE];

            using (NetworkStream stream = tcpClient.GetStream())
            using (MemoryStream ms = new MemoryStream())
            {
                while(connected)
                {
                    if (tcpClient.Connected)
                    {
                        if (messages.Count > 0)
                        {
                            Console.WriteLine("Writing.");
                            Message<T, V> messageDequeued = messages.Dequeue();

                            string clientAsString = JsonConvert.SerializeObject(messageDequeued);
                            stream.WriteAsync(Encoding.ASCII.GetBytes(clientAsString), 0, clientAsString.Length);
                            Console.WriteLine(clientAsString);
                        }

                        do
                        {
                            Console.WriteLine("Reading.");
                                
                            try
                            {
                                bytesRead = stream.Read(buffer, 0, buffer.Length);
                            }
                            catch (IOException e)
                            {
                                Console.WriteLine("Server disconnected.");
                                DisconnectedEvent();
                                return;
                            }
                            ms.Write(buffer, 0, bytesRead);
                        } while (stream.DataAvailable);

                        Message<T, V> message = ConvertByteToMessage(ms.ToArray());

                        HijackReadMessage(message);

                        callback(message);
                        ms.SetLength(0);

                        buffer = new byte[BUFFER_SIZE];
                        bytesRead = 0;
                    }
                }
            } 
        }

        public abstract void SetMessageIdHeader(Message<T, V> message);

        public abstract void HijackReadMessage(Message<T, V> message);

        public abstract void DisconnectedEvent();

        private Message<T, V> ConvertByteToMessage(byte[] message)
        {
            string stringMessage = Encoding.ASCII.GetString(message);
            Message<T, V> client = JsonConvert.DeserializeObject<Message<T, V>>(stringMessage);
            SetMessageIdHeader(client);
            Console.WriteLine(JsonConvert.SerializeObject(client));
            return client;
        }
    }
}

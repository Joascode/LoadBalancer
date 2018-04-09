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
    class Chatter<T,V>
    {
        //Dictionary<string, Message<string, string>> clients = new Dictionary<string, Message<string, string>>();
        protected Queue<Message<T, V>> messages = new Queue<Message<T, V>>();
        protected Action<Message<T, V>> callback;

        protected TcpClient tcpClient;

        public string guid = Guid.NewGuid().ToString();

        public string Id { get; set; }

        private const int BUFFER_SIZE = 1024;

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
            int bytesRead;
            byte[] buffer = new byte[BUFFER_SIZE];

            using (NetworkStream stream = tcpClient.GetStream())
            using (MemoryStream ms = new MemoryStream())
            {
                while (tcpClient.Connected)
                {
                    if (messages.Count > 0)
                    {
                        Console.WriteLine("Writing.");
                        Message<T, V> client = messages.Dequeue();
                        string clientAsString = JsonConvert.SerializeObject(client);
                        stream.WriteAsync(Encoding.ASCII.GetBytes(clientAsString), 0, clientAsString.Length);
                        Console.WriteLine(clientAsString);
                    }

                    if (stream.DataAvailable)
                    {
                        do
                        {
                            Console.WriteLine("Reading.");
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                            ms.Write(buffer, 0, bytesRead);
                        } while (stream.DataAvailable);

                        callback(ConvertByteToMessage(ms.ToArray()));
                        ms.SetLength(0);

                        buffer = new byte[BUFFER_SIZE];
                        bytesRead = 0;
                    }
                }
            }
        }

        private Message<T, V> ConvertByteToMessage(byte[] message)
        {
            string stringMessage = Encoding.ASCII.GetString(message);
            Console.WriteLine(stringMessage);
            Message<T, V> client = JsonConvert.DeserializeObject<Message<T, V>>(stringMessage);
            return client;
        }
    }
}

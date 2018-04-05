using Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    class Server
    {
        Dictionary<string, Message<string, string>> clients = new Dictionary<string, Message<string, string>>();
        Queue<Message<string, string>> messages = new Queue<Message<string, string>>();
        Action<Message<string, string>> callback;
        TcpClient server;

        public string guid = Guid.NewGuid().ToString();

        public string Id { get; set; }

        private const int BUFFER_SIZE = 1024;

        public Server(string ip, int port, Action<Message<string,string>> callback)
        {
            Random random = new Random();
            int randomId = random.Next(1, 10);
            Id = randomId.ToString();
            Console.WriteLine($"ServerId: {Id}");

            server = new TcpClient();
            server.Connect(ip, port);
            this.callback = callback;
            RunMessageTask();
        }

        public void AddClient(Message<string, string> client)
        {
            if(client.Headers.TryGetValue("Id", out string Id))
            {
                clients.Add(Id, client);
                Console.WriteLine(client.Body);
                AddMessage(client);
            }
        }

        public void AddMessage(Message<string, string> client)
        {
            messages.Enqueue(client);
        }

        private void RunMessageTask()
        {
            try
            {
                Task.Run(() => HandleMessages());
            }
            catch(Exception e) when (
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

            using (NetworkStream stream = server.GetStream())
            using (MemoryStream ms = new MemoryStream())
            {
                while (server.Connected)
                {
                    if (messages.Count > 0)
                    {
                        Console.WriteLine("Writing.");
                        Message<string, string> client = messages.Dequeue();
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
                        ms.Flush();

                        buffer = new byte[BUFFER_SIZE];
                    }
                }
            }
        }

        private Message<string, string> ConvertByteToMessage(byte[] message)
        {
            string stringMessage = Encoding.ASCII.GetString(message);
            Console.WriteLine(stringMessage);
            Message<string, string> client = JsonConvert.DeserializeObject<Message<string, string>>(stringMessage);
            return client;
        }
        
    }
}

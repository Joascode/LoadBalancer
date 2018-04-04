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
    class ClientChatter
    {
        private TcpClient client;
        private Action<Message> callback;

        public string Id { get; set; }
        public Queue<Message> messages = new Queue<Message>();


        public ClientChatter(TcpClient client, Action<Message> callback)
        {
            this.client = client;
            this.callback = callback;

            Random random = new Random();
            int randomId = random.Next(1, 10);
            Id = randomId.ToString();

            Console.WriteLine($"ClientChatterId: {Id}");

            RunMessageTask();
        }

        public void AddMessage(Message client)
        {
            messages.Enqueue(client);
        }

        private void RunMessageTask()
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

        //TODO: Add method to add chatter ID to received message for management.

        private void HandleMessages()
        {
            int bytesRead;
            byte[] buffer = new byte[1024];

            using (NetworkStream stream = client.GetStream())
            using (MemoryStream ms = new MemoryStream())
            {
                while (client.Connected)
                {
                
                    if (messages.Count > 0)
                    {
                        Message client = messages.Dequeue();
                        string clientAsString = JsonConvert.SerializeObject(client);
                        stream.WriteAsync(Encoding.ASCII.GetBytes(clientAsString), 0, clientAsString.Length);
                    }

                    if(stream.DataAvailable)
                    {
                        do
                        {
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                            ms.Write(buffer, 0, bytesRead);
                        } while (stream.DataAvailable);

                        callback(ConvertByteToMessage(ms.ToArray()));
                        ms.SetLength(0);

                        buffer = new byte[1024];
                        bytesRead = 0;
                    }
                }
            }
        }

        private Message ConvertByteToMessage(byte[] message)
        {
            string stringMessage = Encoding.ASCII.GetString(message);
            Console.WriteLine(stringMessage);
            Message client = JsonConvert.DeserializeObject<Message>(stringMessage);

            return client;
        }
    }
}

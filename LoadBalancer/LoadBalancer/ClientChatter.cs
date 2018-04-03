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
        private Action<byte[]> callback;
        private NetworkStream stream;

        public int Id { get; set; }
        public Queue<ClientMessage> messages = new Queue<ClientMessage>();


        public ClientChatter(TcpClient client, Action<byte[]> callback)
        {
            this.client = client;
            this.callback = callback;

            Random random = new Random();
            int randomId = random.Next(1, 10);
            Id = randomId;

            Console.WriteLine($"ClientChatterId: {Id}");

            RunMessageTask();
        }

        public void AddMessage(ClientMessage client)
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

        /*private void ReceiveMessage()
        {
            byte[] buffer = new byte[1024];

            using (MemoryStream ms = new MemoryStream())
            {
                int bytesRead;
                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, bytesRead);

                } while (stream.DataAvailable);

                byte[] tempBuffer = ms.ToArray();
                ms.Flush();

                callback(tempBuffer);
            }
        }

        public void SendMessage(Client message)
        {
            
        }*/

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
                        ClientMessage client = messages.Dequeue();
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

                        callback(ms.ToArray());
                        ms.SetLength(0);

                        buffer = new byte[1024];
                        bytesRead = 0;
                    }
                }
            }
        }
    }
}

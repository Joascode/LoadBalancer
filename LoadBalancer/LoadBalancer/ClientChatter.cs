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
    class ClientChatter : Chatter<string, string>
    {
        //private TcpClient client;
        //private Action<Message<string, string>> callback;

        //public string Id { get; set; }
        //public Queue<Message<string, string>> messages = new Queue<Message<string, string>>();

        //private const int BUFFER_SIZE = 1024;


        public ClientChatter(TcpClient client, Action<Message<string, string>> callback) : base (client, callback)
        {
            //this.client = client;
            
            //this.callback = callback;

            //Random random = new Random();
            //int randomId = random.Next(1, 10);
            //Id = randomId.ToString();

            Console.WriteLine($"ClientChatterId: {Id}");

            //RunMessageTask();
        }

        public override void SetMessageIdHeader(Message<string, string> message)
        {
            if (!message.Headers.ContainsKey("Id"))
            {
                message.Headers.Add("Id", Id);
            }
        }

        /*public void AddMessage(Message<string, string> client)
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

        //TODO: Clean this method up.
        private void HandleMessages()
        {
            int bytesRead;
            byte[] buffer = new byte[BUFFER_SIZE];

            using (NetworkStream stream = client.GetStream())
            using (MemoryStream ms = new MemoryStream())
            {
                while (client.Connected)
                {
                
                    if (messages.Count > 0)
                    {
                        Message<string, string> client = messages.Dequeue();
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

                        buffer = new byte[BUFFER_SIZE];
                        bytesRead = 0;
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
        }*/
    }
}

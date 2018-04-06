using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    class Chatter<T,V>
    {
        //Dictionary<string, Message<string, string>> clients = new Dictionary<string, Message<string, string>>();
        Queue<Message<string, string>> messages = new Queue<Message<string, string>>();
        Action<Message<string, string>> callback;

        public string guid = Guid.NewGuid().ToString();

        public string Id { get; set; }

        private const int BUFFER_SIZE = 1024;

        public Chatter(string ip, int port, Action<Message<string, string>> callback)
        {
            Random random = new Random();
            int randomId = random.Next(1, 10);
            Id = randomId.ToString();

            this.callback = callback;
            RunMessageTask();
        }

        public void AddMessage(Message<string, string> client)
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

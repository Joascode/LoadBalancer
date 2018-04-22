using Chatter;
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
        public ClientChatter(TcpClient client, Action<Message<string, string>> callback, int timeout = 2000) : base (client, callback, timeout)
        {
            Random random = new Random();
            int randomId = random.Next(1, 200);
            Id = randomId.ToString();

            Console.WriteLine($"ClientChatterId: {Id}");
        }

        public override void SetMessageIdHeader(Message<string, string> message)
        {
            if (!message.Headers.ContainsKey("Id"))
            {
                message.Headers.Add("Id", Id);
            }
        }

        public override void HijackReadMessage(Message<string, string> message)
        {
            Console.WriteLine("Message received from Client: ");
        }

        public override void DisconnectedEvent()
        {
            if(connected == false)
            {
                CloseConnection();
                Console.WriteLine("Client disconnected.");
            }
        }
    }
}

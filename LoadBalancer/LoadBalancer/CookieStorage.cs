using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    /*class CookieStorage : ISessionStorage<Message, string>
    {
        public void AddSession(Message client, string serverId)
        {
            client.Headers.Add("Cookie", serverId);
        }

        public void DeleteSession(Message client)
        {
            if(client.Headers.ContainsKey("Cookie"))
            {
                client.Headers.Remove("Cookie");
            }
        }

        public bool GetServerIdForClient(Message client, out string server)
        {
            if(client.Headers.TryGetValue("Cookie", out string serverId))
            {
                server = serverId;
                return true;
            }
            else
            {
                server = null;
                return false;
            }
        }
    }*/
}

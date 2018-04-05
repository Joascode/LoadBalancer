using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    //TODO: Change to work with interface for reflection purposes.
    /*class SessionStorage : ISessionStorage<string, string>
    {
        public Dictionary<string, string> Sessions { get; set; }

        public SessionStorage()
        {
            Sessions = new Dictionary<string, string>();
        }

        public bool GetServerIdForClient(string clientId, out string serverId)
        {
            if (Sessions.TryGetValue(clientId, out string serverid))
            {
                serverId = serverid;
                return true;
            }
            else
            {
                serverId = null;
                return false;
            }
        }

        /// <summary>
        /// Adds a client to a session with a server.
        /// </summary>
        /// <param name="clientId">String representation of the client's Id.</param>
        /// <param name="serverId">String representation of the server's Id to which to connect to.</param>
        public void AddSession(string clientId, string serverId)
        {
            Sessions.Add(clientId, serverId);
        }

        public void DeleteSession(string clientId)
        {
            Sessions.Remove(clientId);
        }
    }*/
}

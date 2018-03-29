﻿using Newtonsoft.Json;
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
        Dictionary<int, Client> clients = new Dictionary<int, Client>();
        Queue<Client> messages = new Queue<Client>();
        Action<byte[]> callback;
        TcpClient server;

        public static int Id { get; set; }

        public Server(string ip, int port, Action<byte[]> callback)
        {
            Id++;
            server = new TcpClient();
            server.Connect(ip, port);
            this.callback = callback;
            RunMessageTask();
        }

        public void AddClient(Client client)
        {
            clients.Add(client.Id, client);
            AddMessage(client);
        }

        public void AddMessage(Client client)
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

        private void HandleMessages()
        {
            int bytesRead;
            byte[] buffer = new byte[1024];

            while(server.Connected)
            {
                using (NetworkStream stream = server.GetStream())
                using(MemoryStream ms = new MemoryStream())
                {
                    if (messages.Count > 0)
                    {
                        Client client = messages.Dequeue();
                        string clientAsString = JsonConvert.SerializeObject(client);
                        stream.WriteAsync(Encoding.ASCII.GetBytes(clientAsString), 0, clientAsString.Length);
                    }

                    do
                    {
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                        ms.Write(buffer, 0, bytesRead);
                    } while (stream.DataAvailable);

                    callback(ms.ToArray());
                    ms.Flush();

                    buffer = new byte[1024];
                }
            }
        }
        
    }
}

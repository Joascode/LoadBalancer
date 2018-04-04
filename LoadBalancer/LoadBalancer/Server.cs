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
        Dictionary<string, Message> clients = new Dictionary<string, Message>();
        Queue<Message> messages = new Queue<Message>();
        Action<byte[]> callback;
        TcpClient server;

        public string guid = Guid.NewGuid().ToString();

        public string Id { get; set; }

        public Server(string ip, int port, Action<byte[]> callback)
        {
            Random random = new Random();
            int randomId = random.Next(1, 10);
            Id = randomId;
            Console.WriteLine($"ServerId: {Id}");

            server = new TcpClient();
            server.Connect(ip, port);
            this.callback = callback;
            RunMessageTask();
        }

        public void AddClient(Message client)
        {
            if(client.Headers.TryGetValue("Id", out string Id))
            {
                clients.Add(Id, client);
                Console.WriteLine(client.Body);
                AddMessage(client);
            }
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

            using (NetworkStream stream = server.GetStream())
            using (MemoryStream ms = new MemoryStream())
            {
                while (server.Connected)
                {
                    if (messages.Count > 0)
                    {
                        Console.WriteLine("Writing.");
                        Message client = messages.Dequeue();
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

                        callback(ms.ToArray());
                        ms.Flush();

                        buffer = new byte[1024];
                    }
                }
            }
        }
        
    }
}

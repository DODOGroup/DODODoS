using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace DODODoS{
    static class littleChat{
        public static readonly string Signature = "?-Dodo-?";
        public static void Send(string s){
            var udp = new UdpClient();
            var mess = UTF8Encoding.UTF8.GetBytes(Signature + udp + Signature);
            udp.Send(mess, mess.Length, new IPEndPoint(IPAddress.Broadcast, 666));
        }
        public static string Recive()
        {
            var udp = new UdpClient();
            var ipe = new IPEndPoint(IPAddress.Any,666);
            var mess = UTF8Encoding.UTF8.GetString(udp.Receive(ref ipe));
            var temp = Regex.Match(mess, Signature + "*" + Signature);
            if (temp.Success)
            {
                return temp.Value;
            }
            return Recive();
        }
    }
}

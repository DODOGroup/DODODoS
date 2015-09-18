using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Chat{
    static class littleChat{
        public static readonly string Signature = "";
             
        static IPEndPoint ipe = new IPEndPoint(IPAddress.Any, 666);
        static UdpClient udp = new UdpClient(ipe);

        public static void Send(string s){
            var udp = new UdpClient();
            var mess = UTF8Encoding.UTF8.GetBytes(Signature + s + Signature);
            udp.Send(mess, mess.Length, new IPEndPoint(IPAddress.Broadcast, 666));
        }
        public static string Receive()
        {
            var mess = UTF8Encoding.UTF8.GetString(udp.Receive(ref ipe));
            var temp = Regex.Match(mess, Signature + ".*" + Signature);
            if (temp.Success)
            {
                return temp.ToString();
            }
            return Receive();
        }
    }
    public static class consoleInterface {
        public static object obj = new object();
        public static void DrawTop(string s) {
            lock (obj) { 
                Console.SetCursorPosition(0, 0);
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                for (int i = 0; i < Console.WindowWidth; i++)
                {
                    Console.Write(" ");
                }
                Console.Write("  " + s);
                for (int i = 0; i < Console.WindowWidth*2  - s.Length-2; i++) {
                    Console.Write(" ");                
                }
                Console.CursorTop = Console.WindowHeight-1;
                for (int i = 0; i < Console.WindowWidth; i++) {
                    Console.Write(" ");
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public static void WriteDirect(int Top, int Left,string message) {
            lock (obj) { 
                Console.CursorLeft = Left;
                Console.CursorTop = Top;
                for (int i = 0; i < Console.WindowHeight-2 && i<(message??"").Length; i++) {
                    Console.Write(message[i]);
                }
            }
        }
        public static void ClearLine(int top) {
            lock (obj) { 
                Console.SetCursorPosition(0, top);
                for (int i = 0; i < Console.WindowWidth; i++)
                {
                    Console.Write(" ");
                }
            }
        }
    }
}

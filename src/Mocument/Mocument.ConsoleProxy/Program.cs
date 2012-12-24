using System;
using System.Configuration;
using Mocument.IPC;
using Mocument.ReverseProxyServer;

namespace Mocument.ConsoleProxy
{
    internal class Program
    {
        private static Server _server;

        private static void Main(string[] args)
        {
            // #TODO: i am going to put the IPC in here so i don't have to refactor all of the tests right this moment but it needs to go in the server

            string ipcPath = ConfigurationManager.AppSettings["ipcPath"];
            string ipcChannel = ConfigurationManager.AppSettings["ipcChannel"];

            var ipc = IpcChannel.Create(ipcPath, (IpcChannelSide)Enum.Parse(typeof(IpcChannelSide), ipcChannel, true));
            ipc.DataReceived += new EventHandler<IpcDataRecievedEventArgs>(IPCDataReceived);
            int port = int.Parse(ConfigurationManager.AppSettings["proxyPort"]);
            string libraryPath = ConfigurationManager.AppSettings["contextPath"];
            bool lockDown = bool.Parse(ConfigurationManager.AppSettings["proxyLockDown"]);


            _server = new Server(libraryPath, port, lockDown);
            Console.CancelKeyPress += ConsoleCancelKeyPress;
            _server.Start();
            Console.WriteLine("Hit CTRL+C to end session.");


            bool bDone = false;
            do
            {
                Console.WriteLine(
                    "\nEnter a command [G=Collect Garbage; Q=Quit]:");
                Console.Write(">");
                ConsoleKeyInfo cki = Console.ReadKey();
                Console.WriteLine();
                switch (cki.KeyChar)
                {
                    case 'g':
                        Console.WriteLine("Working Set:\t" + Environment.WorkingSet.ToString("n0"));
                        Console.WriteLine("Begin GC...");
                        GC.Collect();
                        Console.WriteLine("GC Done.\nWorking Set:\t" + Environment.WorkingSet.ToString("n0"));
                        break;

                    case 'q':
                        bDone = true;
                        _server.Stop();
                        break;
                }
            } while (!bDone);
        }

        static void IPCDataReceived(object sender, IpcDataRecievedEventArgs e)
        {
            Console.WriteLine("IPC: {0}", e.Message);
        }


        private static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _server.Stop();
        }
    }
}
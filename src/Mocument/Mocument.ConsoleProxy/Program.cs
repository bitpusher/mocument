using System;
using System.Configuration;

using Mocument.ReverseProxyServer;

namespace Mocument.ConsoleProxy
{
    internal class Program
    {
        private static Server _server;

        private static void Main(string[] args)
        {
            // #TODO: i am going to put the IpcCommunicator in here so i don't have to refactor all of the tests right this moment but it needs to go in the server

            int port = int.Parse(ConfigurationManager.AppSettings["proxyPort"]);

            bool secured = bool.Parse(ConfigurationManager.AppSettings["proxySecured"]);

            var store = new DataAccess.SQLite.SQLiteStore("mocument");
            _server = new Server(port, secured, store);
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




        private static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _server.Stop();
        }
    }
}
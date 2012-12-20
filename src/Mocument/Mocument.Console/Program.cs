using System;
using Mocument.Server;

namespace Mocument.Console
{
    internal class Program
    {
        private static MocumentServer _server;


        private static void Main(string[] args)
        {
            string libraryPath = args[0];
            int port = int.Parse(args[1]);

            System.Console.CancelKeyPress += ConsoleCancelKeyPress;

            _server = new MocumentServer(libraryPath, port);
            _server.Start();
            System.Console.WriteLine("Hit CTRL+C to end session.");


            bool bDone = false;
            do
            {
                System.Console.WriteLine(
                    "\nEnter a command [G=Collect Garbage; Q=Quit]:");
                System.Console.Write(">");
                ConsoleKeyInfo cki = System.Console.ReadKey();
                System.Console.WriteLine();
                switch (cki.KeyChar)
                {
                    case 'g':
                        System.Console.WriteLine("Working Set:\t" + Environment.WorkingSet.ToString("n0"));
                        System.Console.WriteLine("Begin GC...");
                        GC.Collect();
                        System.Console.WriteLine("GC Done.\nWorking Set:\t" + Environment.WorkingSet.ToString("n0"));
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
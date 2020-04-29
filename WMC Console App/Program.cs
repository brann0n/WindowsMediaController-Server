using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows_Media_Controller_Library;
using Windows_Media_Controller_Library.Models.Data;
using Windows_Media_Controller_Library.Modules;

namespace WMC_Console_App
{
    class Program
    {
        public static Server Server;
		public static MusicCommandHandler handler;

        static void Main(string[] args)
        {
			InitServer();
			Console.ReadLine();
        }


		public static void InitServer()
		{
			Program.Server = new Server(IPAddress.Any, 2048); //start server on any ip with 2kb buffer
			Program.Server.ClientConnected += Server_ClientConnected;
			Program.Server.ClientDisconnected += Server_ClientDisconnected;
			Program.Server.ConnectionBlocked += Server_ConnectionBlocked;
			Program.Server.MessageReceived += Server_MessageReceived;
			Program.Server.Start();
			Program.handler = new MusicCommandHandler();

			WriteLine("Server started");

			//start the broadcast receiver on a new thread
			Task.Run(() => new AutoDiscovery(9718, "WMC Android App", "WMC Server v1.0.0"));
		}


		private static void Server_MessageReceived(Client c, TransferCommandObject model)
		{
			WriteLine($"Client #{c.GetClientID()} received data: {model.Command}{{{model.Value}}}");
			if (handler.Invoke(c, model))
				WriteLine("Execute Success");
			else
				WriteLine("Execute Failed");
		}

		private static void Server_ConnectionBlocked(IPEndPoint endPoint)
		{
			WriteLine($"Connection refused for: {endPoint.Address.ToString()}");
		}

		private static void Server_ClientDisconnected(Client c)
		{
			WriteLine($"Client Disconnected: {c.ToString()}");
		}

		private static void Server_ClientConnected(Client c)
		{
			WriteLine($"Client Connected: {c.ToString()}");
		}

		public static void WriteLine(string message)
		{
			Console.WriteLine(message);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Threading;
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
		private static Timer timer;
		private static string PrevSong = "";

        static void Main(string[] args)
        {
			InitServer();

			Console.ReadLine();
        }


		public static void InitServer()
		{
			Program.Server = new Server(IPAddress.Any); //start server on any ip with 2kb buffer
			Program.Server.ClientConnected += Server_ClientConnected;
			Program.Server.ClientDisconnected += Server_ClientDisconnected;
			Program.Server.ConnectionBlocked += Server_ConnectionBlocked;
			Program.Server.MessageReceived += Server_MessageReceived;
			Program.Server.Start();
			Program.handler = new MusicCommandHandler();

			WriteLine("Server started", ConsoleColor.Red);

			//start the broadcast receiver on a new thread
			Task.Run(() => new AutoDiscovery(9718, "WMC Android App", "WMC Server v1.0.0"));
			WriteLine("Starting Song Discovery Loop", ConsoleColor.Yellow);
			timer = new Timer(Timer_Tick, null, 1000, 3000);
			
		}

		private static void Timer_Tick(Object state)
		{
			//check for music and send it to all clients		
			string song = MusicController.GetTrackInfo("Google Play Music Desktop Player");

			if (song != PrevSong)
			{
				PrevSong = song;
				Server.SendDataObjectToAll(0x1C, ClientServerPipeline.BufferSerialize(new TransferCommandObject { Command = "NowPlaying", Value = song }));
				WriteLine("Now Playing: " + song, ConsoleColor.Cyan);
			}		
		}


		private static void Server_MessageReceived(Client c, TransferCommandObject model, DataEventType type)
		{
			WriteLine($"Client #{c.GetClientID()} received data: {model.Command}{{{model.Value}}}", ConsoleColor.Yellow);

			switch (type)
			{
				case DataEventType.DATA:
					break;
				case DataEventType.RESPONSE:
					if(model.Command == "Login" && model.Value == "Accepted")
					{
						//TransferCommandObject m = new TransferCommandObject();
						//m.Command = "Debug";
						//m.Value = "TestVolume";
						//Program.Server.SendDataObjectToAll(ClientServerPipeline.BufferSerialize(m));
					}
					break;
				case DataEventType.COMMAND:
					if (handler.InvokeCommand(c, model))
						WriteLine("Command Completed");
					else
						WriteLine("Command Failed");
					break;
			}			
		}

		private static void Server_ConnectionBlocked(IPEndPoint endPoint)
		{
			WriteLine($"Connection refused for: {endPoint.Address.ToString()}", ConsoleColor.Yellow);
		}

		private static void Server_ClientDisconnected(Client c)
		{
			WriteLine($"Client Disconnected: {c.ToString()}", ConsoleColor.Yellow);
		}

		private static void Server_ClientConnected(Client c)
		{
			WriteLine($"Client Connected: {c.ToString()}", ConsoleColor.Yellow);
		}

		/// <summary>
		/// Overload of the writeline function that displays current time in front of the text to write
		/// </summary>
		/// <param name="line"></param>
		/// <param name="color"></param>
		public static void WriteLine(string line, ConsoleColor color = ConsoleColor.White)
		{
			string timeFormat = $"[{DateTime.Now.ToString("HH:MM:ss")}] ";
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write(timeFormat);
			Console.ForegroundColor = color;
			Console.WriteLine(line);
			Console.ResetColor();
		}
	}
}

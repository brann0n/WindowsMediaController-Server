using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Media_Controller_Library
{
	public class AutoDiscovery
	{
		public AutoDiscovery(int port, string LocalPassword, string RemotePassword)
		{
			var responseData = Encoding.ASCII.GetBytes(LocalPassword);
			while (true)
			{
				UdpClient server = new UdpClient();
				server.EnableBroadcast = true;
				server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				server.Client.Bind(new IPEndPoint(IPAddress.Any, port));
				IPEndPoint clientEp = new IPEndPoint(IPAddress.Any, port);
				server.Client.EnableBroadcast = true;
				byte[] clientRequestData = server.Receive(ref clientEp);
				string clientRequest = Encoding.ASCII.GetString(clientRequestData);
				if (clientRequest == RemotePassword)
				{
					Console.WriteLine($"Received {clientRequest} from {clientEp.Address}, sending response: { responseData} ");

					server.Send(responseData, responseData.Length, clientEp);
					server.Close();
				}
				else
				{
					Console.WriteLine($"Received {clientRequest} from {clientEp.Address}, ignoring!");
				}
			}
		}
	}
}

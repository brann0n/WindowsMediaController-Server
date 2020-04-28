using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows_Media_Controller_Library.Models;
using Windows_Media_Controller_Library.Models.Data;

namespace Windows_Media_Controller_Library
{
    public class Server
    {
		private IPAddress ip;
		private int dataSize;
		private byte[] data;
		private Socket serverSocket;
		private bool acceptIncomingConnections;
		private uint clientCount = 0;

		/// <summary>
		/// Contains all connected clients indexed
		/// by their socket.
		/// </summary>
		private Dictionary<Socket, Client> clients;

		//delegates
		public delegate void ConnectionEventHandler(Client c);
		public delegate void ConnectionBlockedEventHandler(IPEndPoint endPoint);
		public delegate void ClientMessageReceivedHandler(Client c, DataRespondModel model);

		/// <summary>
		/// Occurs when a client is connected.
		/// </summary>
		public event ConnectionEventHandler ClientConnected;

		/// <summary>
		/// Occurs when a client is disconnected.
		/// </summary>
		public event ConnectionEventHandler ClientDisconnected;

		/// <summary>
		/// Occurs when an incoming connection is blocked.
		/// </summary>
		public event ConnectionBlockedEventHandler ConnectionBlocked;

		public event ClientMessageReceivedHandler MessageReceived;

		public Server(IPAddress ip, int dataSize = 1024)
		{
			this.ip = ip;
			this.dataSize = dataSize;
			this.data = new byte[dataSize];
			this.clients = new Dictionary<Socket, Client>();
			this.acceptIncomingConnections = true;
			this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		/// <summary>
		/// Starts the server.
		/// </summary>
		public void Start()
		{
			serverSocket.Bind(new IPEndPoint(ip, 1222));
			serverSocket.Listen(0);
			serverSocket.BeginAccept(new AsyncCallback(HandleIncomingConnection), serverSocket);
		}

		/// <summary>
		/// Stops the server.
		/// </summary>
		public void Stop()
		{
			serverSocket.Close();
		}

		/// <summary>
		/// Handles and accepts all incoming connections.
		/// </summary>
		/// <param name="result"></param>
		private void HandleIncomingConnection(IAsyncResult result)
		{
			try
			{
				Socket oldSocket = (Socket)result.AsyncState;

				if (acceptIncomingConnections)
				{
					Socket newSocket = oldSocket.EndAccept(result);

					uint clientID = clientCount++;
					Client client = new Client(clientID, (IPEndPoint)newSocket.RemoteEndPoint);
					clients.Add(newSocket, client);

					ClientConnected(client);

					DataSendModel m = new DataSendModel();
					m.Object = new ClientAuthenticateObject("UserName");
					SendDataObjectToSocket(newSocket, ClientServerPipeline.Serialize(m));

					serverSocket.BeginAccept(new AsyncCallback(HandleIncomingConnection), serverSocket);
				}
				else
				{
					ConnectionBlocked((IPEndPoint)oldSocket.RemoteEndPoint);
				}
			}

			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		/// <summary>
		/// Returns whether incoming connections
		/// are allowed.
		/// </summary>
		/// <returns>True is connections are allowed;
		/// false otherwise.</returns>
		public bool IncomingConnectionsAllowed()
		{
			return acceptIncomingConnections;
		}

		/// <summary>
		/// Denies the incoming connections.
		/// </summary>
		public void DenyIncomingConnections()
		{
			this.acceptIncomingConnections = false;
		}

		/// <summary>
		/// Allows the incoming connections.
		/// </summary>
		public void AllowIncomingConnections()
		{
			this.acceptIncomingConnections = true;
		}

		/// <summary>
		/// Gets the client by socket.
		/// </summary>
		/// <param name="clientSocket">The client's socket.</param>
		/// <returns>If the socket is found, the client instance
		/// is returned; otherwise null is returned.</returns>
		private Client GetClientBySocket(Socket clientSocket)
		{
			if (!clients.TryGetValue(clientSocket, out Client c))
				c = null;

			return c;
		}

		/// <summary>
		/// Gets the socket by client.
		/// </summary>
		/// <param name="client">The client instance.</param>
		/// <returns>If the client is found, the socket is
		/// returned; otherwise null is returned.</returns>
		public Socket GetSocketByClient(Client client)
		{
			Socket s;

			s = clients.FirstOrDefault(x => x.Value.GetClientID() == client.GetClientID()).Key;

			return s;
		}

		/// <summary>
		/// Kicks the specified client from the server.
		/// </summary>
		/// <param name="client">The client.</param>
		public void KickClient(Client client)
		{
			Socket s = GetSocketByClient(client);
			if (s != null)
			{
				CloseSocket(s);
				ClientDisconnected(client);
			}
		}

		/// <summary>
		/// Closes the socket and removes the client from
		/// the clients list.
		/// </summary>
		/// <param name="clientSocket">The client socket.</param>
		private void CloseSocket(Socket clientSocket)
		{
			clientSocket.Close();
			clients.Remove(clientSocket);
		}

		/// <summary>
		/// Receives and processes data from a socket.
		/// It triggers the message received event in
		/// case the client pressed the return key.
		/// </summary>
		private void ReceiveData(IAsyncResult result)
		{
			try
			{
				Socket clientSocket = (Socket)result.AsyncState;
				Client client = GetClientBySocket(clientSocket);

				int bytesReceived = clientSocket.EndReceive(result);

				if (bytesReceived == 0)
				{
					CloseSocket(clientSocket);
					serverSocket.BeginAccept(new AsyncCallback(HandleIncomingConnection), serverSocket);
				}
				else if (data[0] < 0xF0)
				{
					var _data = new DataTransferObject();
					_data.Data = data;
					HandleIncomingData(ClientServerPipeline.Deserialize(_data), client);
					clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
				}
				else
					clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
			}
			catch (SocketException e2)
			{
				Socket clientSocket = (Socket)result.AsyncState;
				Client client = GetClientBySocket(clientSocket);
				KickClient(client);
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error occured: {e.Message}");
			}
		}

		private void HandleIncomingData(object dObj, Client client)
		{
			if (dObj.GetType().isType(typeof(DataRespondModel)))
			{
				DataRespondModel obj = (DataRespondModel)dObj;
				client.RespondModels.Add(obj);
				MessageReceived?.Invoke(client, obj);
			}
			else
			{
				Console.WriteLine("Object type not supported");
			}
		}

		/// <summary>
		/// Sends a text message to the specified
		/// socket.
		/// </summary>
		/// <param name="s">The socket.</param>
		/// <param name="message">The message.</param>
		private void SendMessageToSocket(Socket s, string message)
		{
			byte[] data = Encoding.ASCII.GetBytes(message);
			SendBytesToSocket(s, data);
		}

		/// <summary>
		/// Sends a text message to the specified
		/// socket.
		/// </summary>
		/// <param name="s">The socket.</param>
		/// <param name="message">The message.</param>
		public void SendDataObjectToSocket(Socket s, DataTransferObject message)
		{
			SendBytesToSocket(s, message.Data);
		}

		/// <summary>
		/// Sends bytes to the specified socket.
		/// </summary>
		/// <param name="s">The socket.</param>
		/// <param name="data">The bytes.</param>
		private void SendBytesToSocket(Socket s, byte[] data)
		{
			s.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendData), s);
		}

		/// <summary>
		/// Sends data to a socket.
		/// </summary>
		private void SendData(IAsyncResult result)
		{
			try
			{
				Socket clientSocket = (Socket)result.AsyncState;

				clientSocket.EndSend(result);

				clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
			}

			catch (Exception e)
			{
			}
		}


		public void SendDataObjectToAll(DataTransferObject message)
		{
			foreach (Socket s in clients.Keys)
			{
				try
				{
					SendDataObjectToSocket(s, message);
				}
				catch { }
			}
		}

		public List<Client> GetClients()
		{
			return clients.Values.ToList();
		}
	}
}

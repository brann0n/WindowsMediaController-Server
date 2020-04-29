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
        private List<DataBufferModel> Buffers;

        /// <summary>
        /// Contains all connected clients indexed
        /// by their socket.
        /// </summary>
        private Dictionary<Socket, Client> clients;

        //delegates
        public delegate void ConnectionEventHandler(Client c);
        public delegate void ConnectionBlockedEventHandler(IPEndPoint endPoint);
        public delegate void ClientMessageReceivedHandler(Client c, TransferCommandObject model);

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
            this.Buffers = new List<DataBufferModel>();
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

                    //Send the other client an authentication request
                    TransferCommandObject m = new TransferCommandObject();
                    m.Command = "Login";
                    m.Value = "x_891$UI.()";
                    SendDataObjectToSocket(newSocket, ClientServerPipeline.BufferSerialize(m));

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

                //guidelines for the received data:
                // first byte is check:
                //											0x1A: Response
                //											0x1B: Command
                //											0x1C: Data
                // second byte is data length:
                //											0x01: 1x 2029 bytes sets
                //											0x1B: 27x 2029 bytes sets
                //											0x22: 34x 2029 bytes sets etc...
                // third byte is series:
                //											0x01: 1/N byte set
                //											0x1A: 26/N byte set etc...
                // the next 16 bytes are unique id:
                //											0x00112233445566778899AABBCCDDFF00
                if (bytesReceived == 0)
                {
                    CloseSocket(clientSocket);
                    serverSocket.BeginAccept(new AsyncCallback(HandleIncomingConnection), serverSocket);
                }
                else if (data[0] == 0x1A || data[0] == 0x1B || data[0] == 0x1C) //less than the max byte check
                {
                    int length = data[1];
                    int series = data[2];
                    Guid guid = new Guid(data.SubArray(3, 19));
                    if (length > 1)
                    {
                        //find the buffer
                        DataBufferModel buffer = Buffers.FirstOrDefault(n => n.DataId == guid);
                        if (buffer != null)
                        {
                            buffer.BufferedData.Add(series, data.SubArray(20, 2028));
                            buffer.LatestSeries = series;
                        }
                        else
                        {
                            //create a new buffer
                            buffer = new DataBufferModel();
                            buffer.BufferedData.Add(series, data.SubArray(20, 2028));
                            buffer.DataId = guid;
                            buffer.SeriesLength = length;
                            buffer.LatestSeries = series;
                            Buffers.Add(buffer);
                        }
                        if (buffer.BufferedData.Count == buffer.SeriesLength)
                        {
                            //process data
                            if (HandleIncomingData(ClientServerPipeline.BufferDeserialize<TransferCommandObject>(buffer), client))
                            {
                                //data processed success, remove this request from the request buffer
                            }
                            else
                            {
                                //something went wrong, report this to the user
                                
                            }
                        }
                    }
                    else
                    {


                        //just one pack of data :)
                    }

                    switch (data[0])
                    {
                        case 0xAA:

                            break;
                        case 0xBB:
                            break;
                        case 0xCC:
                            break;
                    }
                    //HandleIncomingData(ClientServerPipeline.Deserialize(_data), client);
                    clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
                }
                else
                    clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
            }
            catch (SocketException)
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

        private bool HandleIncomingData(object dObj, Client client)
        {
            if (dObj.GetType().isType(typeof(TransferCommandObject)))
            {
                TransferCommandObject obj = (TransferCommandObject)dObj;
                //send the transfer object to the command handler
                //client.RespondModels.Add(obj);
                MessageReceived?.Invoke(client, obj);
                return true;
            }
            else
            {
                Console.WriteLine("Object type not supported");
                return false;
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
        public void SendDataObjectToSocket(Socket s, DataBufferModel message)
        {
            Console.WriteLine("Sending data with id: " + message.DataId.ToString());
            foreach (KeyValuePair<int, byte[]> item in message.BufferedData)
            {
                byte[] sendArray = new byte[] { 0x1B, (byte)message.SeriesLength, (byte)item.Key };
                sendArray = sendArray.Concat(message.DataId.ToByteArray()).Concat(item.Value).ToArray();
                SendBytesToSocket(s, sendArray);
            }
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
                Console.WriteLine("Error occured: " + e.Message.Substring(0, 20));
            }
        }


        public void SendDataObjectToAll(DataBufferModel message)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Media_Controller_Library
{
	public class Client
	{
		private IPEndPoint endPoint;
		private uint id;
		private string receivedData;
		private DateTime connectedAt;
		private string Name;


		public Client(uint id, IPEndPoint pAddressEndpoint)
		{
			this.id = id;
			this.connectedAt = DateTime.Now;
			this.endPoint = pAddressEndpoint;
			SetName($"Client #{id}");
		}

		public string GetName()
		{
			return Name;
		}

		public void SetName(string name)
		{
			Name = name;
		}


		/// <summary>
		/// Gets the client identifier.
		/// </summary>
		/// <returns>Client's identifier.</returns>
		public uint GetClientID()
		{
			return id;
		}

		/// <summary>
		/// Gets the remote address.
		/// </summary>
		/// <returns>Client's remote address.</returns>
		public IPEndPoint GetRemoteAddress()
		{
			return endPoint;
		}

		/// <summary>
		/// Gets the client's last received data.
		/// </summary>
		/// <returns>Client's last received data.</returns>
		public string GetReceivedData()
		{
			return receivedData;
		}
		/// <summary>
		/// Sets the client's last received data.
		/// </summary>
		/// <param name="newReceivedData">The new received data.</param>
		public void SetReceivedData(string newReceivedData)
		{
			this.receivedData = newReceivedData;
		}

		/// <summary>
		/// Appends a string to the client's last
		/// received data.
		/// </summary>
		/// <param name="dataToAppend">The data to append.</param>
		public void AppendReceivedData(string dataToAppend)
		{
			this.receivedData += dataToAppend;
		}

		/// <summary>
		/// Removes the last character from the
		/// client's last received data.
		/// </summary>
		public void RemoveLastCharacterReceived()
		{
			receivedData = receivedData.Substring(0, receivedData.Length - 1);
		}

		/// <summary>
		/// Resets the last received data.
		/// </summary>
		public void ResetReceivedData()
		{
			receivedData = string.Empty;
		}

		public override string ToString()
		{
			string ip = string.Format("{0}:{1}", endPoint.Address.ToString(), endPoint.Port);

			string res = string.Format("Client #{0} (From: {1}, Connection time: {2})", id, ip, connectedAt);

			return res;
		}
	}
}

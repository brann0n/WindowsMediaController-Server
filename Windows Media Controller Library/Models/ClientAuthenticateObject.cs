using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows_Media_Controller_Library.Models.Data;
using Windows_Media_Controller_Library.Modules;

namespace Windows_Media_Controller_Library.Models
{
	[Serializable]
	public class ClientAuthenticateObject : IResponseInterface
	{
		private string RespondAction;

		public ClientAuthenticateObject(string rAction)
		{
			RespondAction = rAction;
		}

		public void Respond(Socket s)
		{
			switch (RespondAction)
			{
				case "UserName":
					ExecuteResponse(s, new DataRespondModel(RespondAction));
					break;
				case "Drives":
					ExecuteResponse(s, new DataRespondModel(RespondAction));
					break;
				default:
					ExecuteResponse(s, new DataRespondModel());
					break;
			}
		}

		private void ExecuteResponse(Socket s, object o)
		{
			DataTransferObject obj = ClientServerPipeline.Serialize(o);
			s.Send(obj.Data, 0, obj.Data.Length, SocketFlags.None);
		}
	}
}

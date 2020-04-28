using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Media_Controller_Library.Modules
{
	public interface IResponseInterface
	{
		void Respond(Socket s);
	}
}

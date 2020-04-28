using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Media_Controller_Library.Models.Data
{
	public class DataTransferObject
	{
		public byte[] Data { get; set; }

		public DataTransferObject()
		{
			Data = new byte[2048];
		}
	}
}

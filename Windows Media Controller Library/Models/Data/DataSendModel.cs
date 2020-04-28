using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Media_Controller_Library.Models.Data
{
	[Serializable]
	public class DataSendModel
	{
		public string id { get; set; } //used to identify the request
		public object Object { get; set; }
	}
}

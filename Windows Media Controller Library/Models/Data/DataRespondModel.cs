using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Media_Controller_Library.Models.Data
{
	[Serializable]
	public class DataRespondModel
	{
		public string ResponseType;
		public object ResponseObject;
		public DataRespondModel()
		{
			//just react
			ResponseType = "Heartbeat";
		}

		public DataRespondModel(string type)
		{
			ResponseType = type;
			switch (type)
			{
				case "UserName":
					ResponseObject = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
					break;
				case "Drives":
					string returnString = "Available Drives: ";
					var drives = DriveInfo.GetDrives();
					foreach (DriveInfo info in drives)
					{
						if (info.IsReady)
						{
							returnString += $"({info.Name.Replace("\\", "")}) ";
						}
					}
					ResponseObject = returnString;
					break;
				case "Heartbeat":
					break;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Windows_Media_Controller_Library.Models.Data;

namespace Windows_Media_Controller_Library
{
    class ClientServerPipeline
    {
		public static DataTransferObject Serialize(object anySerializableObject)
		{
			using (var memoryStream = new MemoryStream())
			{
				(new BinaryFormatter()).Serialize(memoryStream, anySerializableObject);
				return new DataTransferObject { Data = memoryStream.ToArray() };
			}
		}

		public static object Deserialize(DataTransferObject message)
		{
			using (var memoryStream = new MemoryStream(message.Data))
				return (new BinaryFormatter()).Deserialize(memoryStream);
		}
	}
}

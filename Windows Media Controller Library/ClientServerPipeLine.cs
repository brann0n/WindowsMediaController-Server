using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Windows_Media_Controller_Library.Models;
using Windows_Media_Controller_Library.Models.Data;

namespace Windows_Media_Controller_Library
{
    public class ClientServerPipeline
    {
        public static DataBufferModel BufferSerialize(TransferCommandObject ObjectToSerialize) //changed serialization into pattern seperated text: <SPLITPATTERN>
        {
            string data = $"{ObjectToSerialize.Command}<SPLITPATTERN>{ObjectToSerialize.Value}";
            byte[] byteArray = Encoding.Default.GetBytes(data);
            DataBufferModel buffer = new DataBufferModel();
            buffer.DataId = Guid.NewGuid();
            int count = 0;
            int bytesLeft = byteArray.Length;
            int index = 0;
            int increment = 2029;
            while (bytesLeft > 0)
            {
                count++;
                byte[] subArray = byteArray.SubArray(index, increment);

                bytesLeft -= increment;
                index += increment;

                buffer.BufferedData.Add(count, subArray);              
            }
            buffer.SeriesLength = count;

            return buffer;
        }

        public static TransferCommandObject BufferDeserialize(DataBufferModel bufferModel)
        {

            if (bufferModel.BufferedData.Count == bufferModel.SeriesLength)
            {
                byte[] fullBuffer = bufferModel.BufferedData[1];

                if(bufferModel.SeriesLength > 1)
                {
                    for (int i = 2; i <= bufferModel.SeriesLength; i++)
                    {
                        fullBuffer = fullBuffer.Concat(bufferModel.BufferedData[i]).ToArray();
                    }
                }

                string data = Encoding.Default.GetString(fullBuffer.Where(n=> n != 0).ToArray()).Replace("\0", "");
                return JsonConvert.DeserializeObject<TransferCommandObject>(data);
            }
            else
            {
                return default;
            }

        }
    }
}

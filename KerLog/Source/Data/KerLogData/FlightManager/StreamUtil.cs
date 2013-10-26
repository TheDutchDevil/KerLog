using KerLogData.FlightData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace KerLogData.FlightManager
{
    public class StreamUtil
    {
        public static void WriteToStream<T>(T objectToWrite, Stream stream)
        {
            byte[] messageBytes = ObjectToByteArray(objectToWrite);
            byte[] messageSizeBytes = IntToByteArray(messageBytes.Length);

            stream.Write(messageSizeBytes, 0, 4);
            stream.Write(messageBytes, 0, messageBytes.Length);
        }

        public static byte[] IntToByteArray(int integer)
        {
            return BitConverter.GetBytes(integer);
        }

        public static byte[] ObjectToByteArray(Object obj)
        {
            if (obj.GetType().Equals(typeof(Flight)))
            {
                return ProtoBufWrapper.ToByteArray(obj as Flight);
            }

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Reads an object of the type t from the stream, according to the format 4 bytes
        /// which represent an int value with the object size, then the actual object.
        /// </summary>
        /// <typeparam name="T">Supports c# primites or a Flight object</typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T ReadObjectFromStream<T>(Stream stream)
        {
            byte[] messageSizeBytes = new byte[4];
            stream.Read(messageSizeBytes, 0, 4);

            int messageSize = BitConverter.ToInt32(messageSizeBytes, 0);

            byte[] messageBytes = new byte[messageSize];
            stream.Read(messageBytes, 0, messageSize);

            return DeserializeObject<T>(messageBytes);
        }

        public static T DeserializeObject<T>(byte[] objectInBytes)
        {
            if (typeof(T).Equals(typeof(Flight)))
            {
                return (T)((object)ProtoBufWrapper.FlightFromByteArray(objectInBytes));
            }
            else
            {
                var ms = new MemoryStream(objectInBytes);
                try
                {
                    var formatter = new BinaryFormatter();
                    return (T)formatter.Deserialize(ms);
                }
                finally
                {
                    ms.Close();
                }
            }
        }
    }
}

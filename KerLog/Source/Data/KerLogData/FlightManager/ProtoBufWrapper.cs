using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KerLogData.FlightData;
using ProtoBuf;

namespace KerLogData.FlightManager
{
    /// <summary>
    /// Wrapper around the protobuf serializer so projects
    /// relying on this assembly won't need to include references
    /// to protobuf to serialize / desirialize
    /// </summary>
    public static class ProtoBufWrapper
    {
        /// <summary>
        /// Basic serialize method, won't override if the file already exists
        /// </summary>
        /// <param name="flight"></param>
        /// <param name="path"></param>
        /// <returns>true if the operation succeeded false if it didn't</returns>
        public static bool SerializeToFile(Flight flight, string path)
        {
            return SerializeToFile(flight, path, false);
        }

        /// <summary>
        /// Basic serialize method
        /// </summary>
        /// <param name="flight"></param>
        /// <param name="path"></param>
        /// <param name="overwrite"></param>
        /// <returns>true if the writing succeeded</returns>
        public static bool SerializeToFile(Flight flight, string path, bool overwrite)
        {
            if (flight == null)
            {
                throw new ArgumentException("Flight cannot be null");
            }

            if (File.Exists(path) && !overwrite)
            {
                return false;
            }

            using (var file = File.Create(path))
            {
                Serializer.Serialize(file, flight);
            }

            return true;
        }

        /// <summary>
        /// Serializes the given flight object to a byte array
        /// </summary>
        /// <param name="flight"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(Flight flight)
        {
            if (flight == null)
            {
                throw new ArgumentException("Flight cannot be null");
            }

            byte[] data;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, flight);
                data = ms.ToArray();
            }

            return data;
        }

        public static Flight FlightFromByteArray(byte [] flightBytes)
        {
            using (MemoryStream stream = new MemoryStream(flightBytes, false))
            {
                stream.Position = 0;
                return Serializer.Deserialize<Flight>(stream);
            }


            using(MemoryStream memStream = new MemoryStream())
            {
                memStream.Write(flightBytes, 0, flightBytes.Length);
                return FlightForStream(memStream);
            }
        }

        public static Flight FlightForStream(Stream stream)
        {
            Flight output;

                output = Serializer.Deserialize<Flight>(stream);

            return output;
        }

        public static Flight FlightForPath(string path)
        {
            Flight output;

            if (!File.Exists(path))
            {
                throw new ArgumentException(string.Format("{0} does not exist!", path));
            }

            using (var file = File.OpenRead(path))
            {
                output = Serializer.Deserialize<Flight>(file);
            }

            return output;
        }
    }
}

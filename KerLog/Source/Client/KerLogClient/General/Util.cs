using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

namespace KerLogClient.General
{
    class Util
    {

        /// <summary>
        /// Returns the time difference between the two datetimes in milliseconds
        /// the subtraction used is second - first so the later Datetime should 
        /// be given as second DateTime
        /// </summary>
        /// <param name="first">The first (earlier) DateTime</param>
        /// <param name="second">The second (later) DateTime</param>
        /// <returns>The time difference in milliseconds</returns>
        public static int DTimeInMilliSecondsDateTimes(DateTime first, DateTime second)
        {
            TimeSpan timespan = second - first;
            return timespan.Milliseconds;
        }

        /// <summary>
        /// Credit goes to http://forum.kerbalspaceprogram.com/threads/40729-RadarAltimeter-Value-Access?p=527378&viewfull=1#post527378
        /// </summary>
        /// <param name="vessel"></param>
        /// <returns></returns>
        public static int VesselHeightFromTerrain(Vessel vessel)
        {
            if (vessel.heightFromTerrain == -1 || vessel.altitude < vessel.heightFromTerrain)
            {
                return (int)vessel.altitude;
            }
            else
            {
                return (int) vessel.heightFromTerrain;
            }
        }

        /// <summary>
        /// http://stackoverflow.com/a/15228587
        /// </summary>
        /// <returns></returns>
        private static string UniqueMachineId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        /// <summary>
        /// http://stackoverflow.com/q/12416249
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string HashSha256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        public static string UniqueHashID()
        {
            return HashSha256(UniqueMachineId());
        }

    }
}

using ClientListener.Properties;
using KerLogData.FlightManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClientListener.KerLogListener.VersionManagement
{
    class VersionChecker
    {
        private static int _minimumSupportedSocketVersion;

        private static int _currentSocketVersion;

        private static bool _correctlySetUp;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Attempts to read the version numbers used when
        /// checking the data and socket versions for clients
        /// from the resources file. Logs an exception if it fails
        /// </summary>
        static VersionChecker()
        {
            try
            {
                _minimumSupportedSocketVersion = int.Parse(SupportedVersion.MinimumSupportedSocketVersion);

                _currentSocketVersion = int.Parse(SupportedVersion.CurrentSocketVersion);

                _correctlySetUp = true;
            }
            catch(FormatException ex)
            {
                log.Fatal("Could not format one of the numbers in the SupportedVersionResources to an int32", ex);
            }
        }

        /// <summary>
        /// Whether or not the version numbers were read correctly from the SupportedVersion
        /// resources file
        /// </summary>
        public static bool IsCorrectlySetUp
        {
            get
            {
                return _correctlySetUp;
            }
        }

        /// <summary>
        /// Attempts to read a version message from the provided steram. If the client
        /// socket version is lower than the minimum supported version it will
        /// disconnect from the client with a disconnect message
        /// </summary>
        /// <param name="stream">The stream from which the socket version should be
        /// read</param>
        /// <param name="timeOutPerRead">The time out for the read operation. An attempt
        /// will be made to read from the stream twice. So the maximum time in milliseconds
        /// this method will block is timeOutPerRead * 2</param>
        /// <returns>True if the socket version of the client was read successfully and
        /// it was higher than the minimum supported version. False if the version number
        /// was not read from the remote client or if the version number read was smaller
        /// than the minimum supported version. Also returns false if IsCorrectlySetUp is
        /// false</returns>
        public static bool VersionIsValidForClient(Stream stream, int timeOutPerRead)
        {
            if(!_correctlySetUp)
            {
                log.Warn("An attempt was made to read a version number from a stream while the VersionChecker was not correctly set up");
                return false;
            }

            int clientSocketVersion = 0;

            try
            {
                log.Debug("Trying to read the version number from the client");
                clientSocketVersion = StreamUtil.ReadObjectFromStream<int>(stream, timeOutPerRead);
            }
            catch (IOException ex)
            {
                log.Info("Could not read the version number from the client", ex);
                StreamUtil.WriteToStream(false, stream);
                StreamUtil.WriteToStream("The version number could not be read. Disconnecting. To fix this, please update the client", stream);
                return false;
            }

            if (clientSocketVersion < _minimumSupportedSocketVersion)
            {
                log.InfoFormat("A client with an unsopported socket version tried to connect to the server, client socket version is {0}, minimum supported socket version {0}. Gracefully disconnecting.", clientSocketVersion, _minimumSupportedSocketVersion);
                StreamUtil.WriteToStream(false, stream);
                StreamUtil.WriteToStream(string.Format("Socket connection version is outdated, please update to a newer version"), stream);
                return false;
            }
            else
            {
                log.DebugFormat("A client with a supported socket version of {0} tried to connect", clientSocketVersion);
            }

            return true;
        }
    }
}

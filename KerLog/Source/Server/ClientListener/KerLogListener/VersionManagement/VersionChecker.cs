using ClientListener.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientListener.KerLogListener.VersionManagement
{
    class VersionChecker
    {
        private static int _minimumSupportedSocketVersion;

        private static int _currentSocketVersion;

        private static int _minimumSupportedDataVersion;

        private static int _currentDataVersion;

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

                _minimumSupportedDataVersion = int.Parse(SupportedVersion.MinimumSupportedDataVersion);

                _currentSocketVersion = int.Parse(SupportedVersion.CurrentSocketVersion);

                _currentDataVersion = int.Parse(SupportedVersion.CurrentDataVersion);

                _correctlySetUp = true;
            }
            catch(FormatException ex)
            {
                log.Fatal("Could not format one of the numbers in the SupportedVersionResources to an int32", ex);
            }
        }
    }
}

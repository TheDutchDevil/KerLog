using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using KSP.IO;

namespace KerLogClient.Configuration
{
    class ConfigurationProvider
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
    (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Configuration _configuration;

        public static IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    LoadConfiguration();
                }
                return _configuration;
            }
        }

        private static Configuration DefaultConfiguration
        {
            get
            {
                return new Configuration();
            }
        }

        public static void LoadConfiguration()
        {
            log.Debug("Loading a configuration");
            _configuration = DefaultConfiguration;
        }

        public static void Save()
        {
            _configuration.Save();
        }
    }
}

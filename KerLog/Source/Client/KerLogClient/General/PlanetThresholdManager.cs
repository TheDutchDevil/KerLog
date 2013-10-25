using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace KerLogClient.General
{
    class PlanetThresholdManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static List<planetsPlanet> _planetsThreshold;
        private static string THRESHOLD_FILE_PATH = "General\\PlanetThreshold.xml";

        static PlanetThresholdManager()
        {
            log.Debug("Reading the PlanetThreshold XML file");
            _planetsThreshold = new List<planetsPlanet>();

            if (!File.Exists(THRESHOLD_FILE_PATH))
            {
                log.Fatal(string.Format("Threshold file could not be found at path {0}", THRESHOLD_FILE_PATH));
                throw new IOException(string.Format("File {0} does not exist!", THRESHOLD_FILE_PATH));              
            }

            XmlSerializer serializer = new XmlSerializer(typeof(planets));

            planets lplanets = null;

            try
            {
                using (StreamReader reader = new StreamReader(THRESHOLD_FILE_PATH))
                {
                     lplanets= (planets)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                log.Fatal("An exception occured while deserializing the threshold file", ex);
                throw ex;
            }

            _planetsThreshold.AddRange(lplanets.Items);
        }

        public static int ThresholdForPlanet(string planetName)
        {
            log.Debug(string.Format("Responding to a threshold query for planet {0}", planetName));

            planetsPlanet output = _planetsThreshold.Single(pt => (pt.name == planetName));

            if (output == null)
            {
                log.Error(string.Format("{0} does not exists in the planet threshold list, please add it to the xml file {1}", planetName, THRESHOLD_FILE_PATH));
                return -1;
            }

            if (output.threshold <= 0)
            {
                log.Error(string.Format("threshold for planet {0} is {1} which is lower than zero", planetName, output.threshold));
                return -1;
            }

            return output.threshold;
        }
    }
}

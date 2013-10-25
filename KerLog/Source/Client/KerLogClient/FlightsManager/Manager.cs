using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerLogClient.Configuration;
using KerLogClient.General;
using KerLogData.FlightData;
using KerLogData.FlightManager;
using KSP.IO;

namespace KerLogClient.FlightsManager
{
    class Manager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static List<Flight> _persistedFlights;

        private static object _persistedFlightsLock;

        private static readonly string FOLDER_NAME = "TemporaryFlightStorage";
        private static readonly string EXTENSION = "flightbin";

        static Manager()
        {
            log.Debug("static constructor of the FlightsManager called");

            _persistedFlightsLock = new object();

            _persistedFlights = new List<Flight>();

            string path = "";
            for (int i = 0; i < ConfigurationProvider.Configuration.PersistedFlights.Count; i++ )
            {
                path = ConfigurationProvider.Configuration.PersistedFlights[i];
                log.Debug(string.Format("Checking persisted flight for path {0}", path));
                Flight flightRead = FlightFromSandbox(path);
                if (flightRead != null)
                {
                    log.Debug("Flight exists, adding to flightsRead");
                    _persistedFlights.Add(flightRead);
                }
                else
                {
                    log.Debug("Flight does not exists, removing it from the configuration");
                    ConfigurationProvider.Configuration.RemovePersistedFlight(path);
                }
            }
        }

        public static void AttemptToSendFlights()
        {
            if (PersistedFlights.Count > 0)
            {
                TCPManager.SendFlightsList(new List<Flight>(PersistedFlights.ToArray()));
            }
        }

        private static List<Flight> PersistedFlights
        {
            get 
            { 
                return Manager._persistedFlights; 
            }
            set 
            {
                lock (_persistedFlightsLock)
                {
                    Manager._persistedFlights = value;
                }
            }
        }

        private static Flight FlightFromSandbox(string fileName)
        {
            log.Debug(string.Format("Loading flight from file {0}", fileName));
            if (!File.Exists<Manager>(fileName))
            {
                log.Debug(string.Format("File {0} does not exist", fileName));
                return null;
            }

            BinaryReader bReader = BinaryReader.CreateForType<Manager>(fileName);
            Flight output;

            using (System.IO.Stream stream = bReader.BaseStream)
            {
                try
                {
                    output = ProtoBufWrapper.FlightForStream(stream);
                }
                catch (Exception ex)
                {
                    log.Fatal(string.Format("Could not read the file from stream {0}", ex.Message), ex);
                    throw;
                }
            }
            
            if(output != null)
            {
                log.Debug("Successfully read the flight data");
            }
            else
            {
                log.Debug("Did not read the flight data");
            }
            
            return output;
        }

        public static void RemoveFlightFromSandbox(Flight flight)
        {
            log.Info(string.Format("Deleting a flight for vessel {0}", flight.VesselName));
            string fullPath = FilenameForFlight(flight);

            if(File.Exists<Manager>(fullPath))
            {
                File.Delete<Manager>(fullPath);

                if(!File.Exists<Manager>(fullPath))
                {
                    log.Info("Flight successfuly removed");
                    lock (_persistedFlightsLock)
                    {
                        ConfigurationProvider.Configuration.RemovePersistedFlight(fullPath);
                        PersistedFlights.Remove(flight);
                    }
                }
                else
                {
                    log.Warn(string.Format("Flight {0} was not properly removed. Path: {1}", flight.VesselName, fullPath));
                }
            }
            else
            {
                log.Warn(string.Format("Flight can not be found at path {0}", fullPath));
                ConfigurationProvider.Configuration.RemovePersistedFlight(fullPath);
            }
        }

        public static void PersistFlightToSandbox(Flight flight)
        {
            log.Info(string.Format("Persisting flight for vessel {0}", flight.VesselName));

            string fullPath = FilenameForFlight(flight);

            KSP.IO.BinaryWriter writer = KSP.IO.BinaryWriter.CreateForType<Manager>(fullPath);
            byte[] flightData = ProtoBufWrapper.ToByteArray(flight);

            log.Debug(string.Format("Persisting the flight to the following file {0}", fullPath));

            try
            {
                writer.Write(flightData);
                    log.Info(string.Format("Succesfully persisted flight to {0}", fullPath));
                    ConfigurationProvider.Configuration.AddPersistedFlight(fullPath);
            }
            catch (Exception ex)
            {
                log.Fatal(string.Format("Error thrown wile trying to persist flight to {0}", fullPath), ex);
                throw;
            }
        }

        private static string FilenameForFlight(Flight flight)
        {
            string fileName = string.Format("Flight {0}", flight.FlightStart.ToString("dd_mm_yyyy mm-HH-ss"));

            string fullPath = string.Format("{0}.{1}", fileName, EXTENSION);
            return fullPath;
        }
    }
}

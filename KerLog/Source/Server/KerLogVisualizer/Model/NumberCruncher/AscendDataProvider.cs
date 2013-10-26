using KerLogData.FlightData;
using KerLogVisualizer.Model.Database;
using KerLogVisualizer.Model.NumberCruncher.DataStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KerLogVisualizer.Model.NumberCruncher
{
    public class AscendDataProvider
    {      


        public static AscendData AscendDataForPlanet(string planetName, bool failedAscends)
        {
            List<Flight> allFlights = new DatabaseConnection().AllFlightsInDatabase();

            List<AscendProfile> profiles = new List<AscendProfile>();
                
            foreach(Flight flight in allFlights)
            {
                profiles.AddRange(flight.AscendProfiles.Where(ap => ap.Planet == planetName && ap.AscendSucceeded != failedAscends));
            }

            return null;
        }
    }
}
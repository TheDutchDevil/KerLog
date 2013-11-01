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
        private const int ASCEND_DATA_POINTS = 60;

        public static AscendData AscendDataForPlanet(string planetName, bool failedAscends)
        {
            List<Flight> allFlights = new DatabaseConnection().AllFlightsInDatabase();

            List<AscendProfile> profiles = new List<AscendProfile>();
                
            foreach(Flight flight in allFlights)
            {
                profiles.AddRange(flight.AscendProfiles.Where(ap => ap.Planet == planetName && ap.AscendSucceeded != failedAscends));
            }

            double averageMaxAscendtime = profiles.Average(ap => ap.FlightLength);

            double[] metPoints = new double[ASCEND_DATA_POINTS];

            double interval = (double) averageMaxAscendtime / (double) (ASCEND_DATA_POINTS - 1);

            long[] heightAtMets = new long[ASCEND_DATA_POINTS];

            for (int i = 0; i < ASCEND_DATA_POINTS; i++)
            {
                metPoints[i] = interval * i;
            }

            for (int i = 0; i < metPoints.Length; i++)
            {
                double percentage = ((double)i * 100d) / ASCEND_DATA_POINTS;

                double met = metPoints[i];
                List<long> heightAtMet = new List<long>();

                foreach (AscendProfile ap in profiles)
                {
                    heightAtMet.Add(ap.HeightAtAscendTime((ap.FlightLength * percentage) / 100d));
                }

                heightAtMets[i] = (long)heightAtMet.Average();
            }

            return new AscendData(heightAtMets, planetName, failedAscends);
        }
    }
}
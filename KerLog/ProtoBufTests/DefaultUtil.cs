using KerLogData.FlightData;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtoBufTests
{
    class DefaultUtil
    {

        public static Flight DefaultFlight
        {
            get
            {
                Flight flight = new Flight("Vessel 1", "Drone");

                flight.StartAscend("Planet", 1, 0);

                for (int i = 0; i < 30; i++)
                {
                    flight.AddAscendPoint(i * 100, i + 1);
                }

                flight.StopAscend(true);
                return flight;
            }
        }

        public static SqlConnection DefaultConnection
        {
            get
            {
                string connString = string.Format("Server={0};Database={1};User Id={2};Password={3};", ConnectionDetail.IP, ConnectionDetail.Database, ConnectionDetail.Username, ConnectionDetail.Password);
                return new SqlConnection(connString);
            }
        }
    }
}

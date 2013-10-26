using KerLogData.FlightData;
using KerLogData.FlightManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace KerLogVisualizer.Model.Database
{
    public class DatabaseConnection
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SqlConnection _connection;

        public DatabaseConnection()
        {
            string connString = ConfigurationManager.ConnectionStrings["TestDBSConnection"].ConnectionString;
            _connection = new SqlConnection(connString);

            log.DebugFormat("Created a new DBS connection with the following connection string {0}", connString);
        }

        public List<Flight> AllFlightsInDatabase()
        {
            List<Flight> output = new List<Flight>();

            string query = "Select FlightData from FlightData";

            SqlCommand command = new SqlCommand(query, _connection);

            try
            {
                _connection.Open();

                using(SqlDataReader reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        log.Debug("Read a flight from the database, converting it to a Flight");
                        output.Add(ProtoBufWrapper.FlightFromByteArray((byte[])reader[0]));
                    }
                }
            }
            catch(SqlException ex)
            {
                log.Error("Something went wrong while attempting to read the flights from the database", ex);
            }
            finally
            {
                this._connection.Close();
            }

            log.DebugFormat("Total amount of flightsread is {0}", output.Count);
            return output;
        }
    }
}
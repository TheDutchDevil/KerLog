using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KerLogData.FlightData;
using KerLogData.FlightManager;
using System.Data.SqlClient;
using System.Data;

namespace ProtoBufTests.Serialization
{
    [TestClass]
    public class SerializeToFile
    {
        public static Flight DefaultFlight
        {
            get
            {
                Flight flight = new Flight("Vessel 1", "Drone");

                flight.StartAscend("Planet", 1);

                for (int i = 0; i < 30; i++)
                {
                    flight.AddAscendPoint(i * 100, i + 1);
                }

                flight.StopAscend(true);
                return flight;
            }
        }

        public SqlConnection DefaultConnection
        {
            get
            {
                string connString = string.Format("Server={0};Database={1};User Id={2};Password={3};", ConnectionDetail.IP, ConnectionDetail.Database, ConnectionDetail.Username, ConnectionDetail.Password);
                return new SqlConnection(connString);
            }
        }

        [TestMethod]
        public void SerializeToFileTest()
        {
            Flight flight = DefaultFlight;

            ProtoBufWrapper.SerializeToFile(flight, "test.bin");

            Flight deserFlight = ProtoBufWrapper.FlightForPath("test.bin");

            Assert.AreEqual(flight.VesselName, deserFlight.VesselName);
            Assert.AreEqual(30, deserFlight.AscendProfiles[0].AscendPoints.Count);
        }

        [TestMethod]
        public void SerializeToByteArray()
        {
            Flight flight = DefaultFlight;

            byte[] flightBytes = ProtoBufWrapper.ToByteArray(flight);

            Flight deserFlight = ProtoBufWrapper.FlightFromByteArray(flightBytes);

            Assert.AreEqual(flight.VesselName, deserFlight.VesselName);
            Assert.AreEqual(30, deserFlight.AscendProfiles[0].AscendPoints.Count);
        }

        [TestMethod]
        public void SerializeToDatabase()
        {
            Flight flight = DefaultFlight;

            SqlConnection connection = DefaultConnection;

            connection.Open();

            using(SqlCommand command = new SqlCommand("DELETE FROM FlightData WHERE SenderHashID = 4", connection))
            {
                command.ExecuteNonQuery();
            }

            using (SqlCommand command = new SqlCommand("INSERT INTO FlightData (InsertTimeStamp, SenderHashID, FlightData) VALUES (GetDate(), 4, @flight)", connection))
            {
                SqlParameter param = command.Parameters.Add("@flight", SqlDbType.VarBinary);
                param.Value = ProtoBufWrapper.ToByteArray(flight);

                command.ExecuteNonQuery();
            }


            Flight deserFlight = null;

            using (SqlCommand command = new SqlCommand("SELECT FlightData FROM FlightData WHERE SenderHashID = 4", connection))
            {

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deserFlight = ProtoBufWrapper.FlightFromByteArray((byte[])reader[0]);
                    }
                }
            }

            connection.Close();

            Assert.AreEqual(flight.VesselName, deserFlight.VesselName);
            Assert.AreEqual(30, deserFlight.AscendProfiles[0].AscendPoints.Count);

        }


    }
}

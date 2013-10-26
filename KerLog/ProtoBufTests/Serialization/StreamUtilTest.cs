using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using KerLogData.FlightData;
using KerLogData.FlightManager;

namespace ProtoBufTests.Serialization
{
    [TestClass]
    public class StreamUtilTest
    {
        [TestMethod]
        public void TestWriteAndReadToStreamNonFlightObject()
        {
            MemoryStream stream = new MemoryStream();

            string testString = "HelloStream";

            StreamUtil.WriteToStream(testString, stream);

            stream.Position = 0;

            string readFromStream = StreamUtil.ReadObjectFromStream<string>(stream);

            Assert.AreEqual(testString, readFromStream);
        }

        [TestMethod]
        public void TestWriteAndReadToStreamFlight()
        {
            MemoryStream stream = new MemoryStream();

            Flight flight = SerializeToFile.DefaultFlight;

            StreamUtil.WriteToStream(flight, stream);
            stream.Position = 0;

            Flight deserFlight = StreamUtil.ReadObjectFromStream<Flight>(stream);

            Assert.AreEqual(flight.VesselName, deserFlight.VesselName);
            Assert.AreEqual(30, deserFlight.AscendProfiles[0].AscendPoints.Count);            
        }
    }
}

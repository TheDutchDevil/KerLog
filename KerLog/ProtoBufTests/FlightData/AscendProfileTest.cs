using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KerLogData.FlightData;

namespace ProtoBufTests.FlightData
{
    [TestClass]
    public class AscendProfileTest
    {
        [TestMethod]
        public void TestHeightAtMet()
        {
            Flight flight = DefaultUtil.DefaultFlight;

            Assert.AreEqual(2850, flight.AscendProfiles[0].HeightAtAscendTime(28.5d));
            Assert.AreEqual(2310, flight.AscendProfiles[0].HeightAtAscendTime(23.1d));
        }
    }
}

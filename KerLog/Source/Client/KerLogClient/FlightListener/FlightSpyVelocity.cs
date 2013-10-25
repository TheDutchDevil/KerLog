using KerLogData.FlightData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerLogClient.FlightListener
{
    class FlightSpyVelocity
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Flight _flight;

        public FlightSpyVelocity(Flight flight)
        {
            _flight = flight;
            FlightSpy.DeltaVUpdateEvent += FlightSpy_DeltaVUpdateEvent;
        }

        ~FlightSpyVelocity()
        {
            FlightSpy.DeltaVUpdateEvent -= FlightSpy_DeltaVUpdateEvent;
        }

        void FlightSpy_DeltaVUpdateEvent(double met, double velocity)
        {
            log.DebugFormat("Adding a new DeltaVPoint MET is {0} Delta V is {1}", met, velocity);
            _flight.AddVelocityPoint(met, velocity);
        }
    }
}

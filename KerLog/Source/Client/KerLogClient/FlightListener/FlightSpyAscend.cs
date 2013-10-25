using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerLogClient.FlightListener
{
    class FlightSpyAscend
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static FlightSpyAscend _flightSpyAscend;

        private FlightSpy _flightSpy;
        private double _metStart;

        public FlightSpyAscend()
        {
            log.Debug("Setting up FlightSpyAscend");
            FlightSpy.AscendStartEvent += FlightSpy_AscendStartEvent;
            FlightSpy.AscendUpdateEvent += FlightSpy_AscendUpdateEvent;
            FlightSpy.AscendStopEvent += FlightSpy_AscendStopEvent;
        }

        void FlightSpy_AscendStopEvent(bool ascendSucceeded)
        {
            log.Debug("Stopping the ascend and unsubscribing from the events");
            _metStart = 0;
            _flightSpy.Flight.StopAscend(ascendSucceeded);

            FlightSpy.AscendStartEvent -= FlightSpy_AscendStartEvent;
            FlightSpy.AscendUpdateEvent -= FlightSpy_AscendUpdateEvent;
            FlightSpy.AscendStopEvent -= FlightSpy_AscendStopEvent;
        }

        void FlightSpy_AscendUpdateEvent(int vesselHeight, double currentMet)
        {
            double dMet = currentMet - _metStart;
            log.Debug(string.Format("Adding a ascend point with a vesselheight of {0} and delta met of {1}", vesselHeight, dMet));
            _flightSpy.Flight.AddAscendPoint(vesselHeight, dMet);
        }

        void FlightSpy_AscendStartEvent(FlightSpy flightSpy, double metStart)
        {
            log.Debug(string.Format("Receiving an AscendStart event with a met of {0}", metStart));
            _flightSpy = flightSpy;
            _metStart = metStart;

            if (_flightSpy.Flight.IsAnyAscendActive)
            {
                log.Warn("An ascend is already active in the linked Flight");
            }
            else
            {
                log.Debug("Starting a new ascend for the flight");
                _flightSpy.Flight.StartAscend(FlightGlobals.currentMainBody.name, metStart);
            }
        }

        public static void PrepareFlightSpyAscend()
        {
            _flightSpyAscend = new FlightSpyAscend();
        }
    }
}

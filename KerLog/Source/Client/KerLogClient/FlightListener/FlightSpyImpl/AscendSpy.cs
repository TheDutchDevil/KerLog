using KerLogClient.General;
using KerLogData.FlightData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerLogClient.FlightListener.FlightSpyImpl
{
    class AscendSpy : IFlightSpy
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const double TIME_BETWEEN_UPDATES = 1.0d;

        private bool _ascendActive;

        private Flight _flight;

        private int _threshold;

        private string _planetname;

        public double TimeBetweenUpdate
        {
            get { return TIME_BETWEEN_UPDATES; }
        }

        public bool PreRequisitesAreMet(Flight flight)
        {
            bool preReqsAreMet = true;

            bool planetManagerSetUp = PlanetThresholdManager.CorrectlySetUp;

            if(!planetManagerSetUp)
            {
                log.Error("Could not set up the FlightAscendListener because the planet threshold manager did get set up correctly");
                preReqsAreMet = false;
            }

            if(flight.IsAnyAscendActive)
            {
                log.Error("Could not set up the FlightAscendListener because the flight already has an active ascend");
                preReqsAreMet = false;
            }

            return preReqsAreMet;
        }

        public bool ShouldStartFlightSpy()
        {
            int threshold = 200;
            log.Debug(string.Format("Checking if ascend is active, heightFromTerrain is {0} threshold is {1} ", Util.VesselHeightFromTerrain(FlightGlobals.ActiveVessel), threshold));
            if (Util.VesselHeightFromTerrain(FlightGlobals.ActiveVessel) < threshold &&
                !_ascendActive)
            {
                if (PlanetThresholdManager.ThresholdForPlanet(FlightGlobals.ActiveVessel.mainBody.name) == -1)
                {
                    log.Warn(string.Format("Detected an ascend from the unknown planet {0} not monitoring it", FlightGlobals.ActiveVessel.mainBody.name));
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void StartFlightSpy(Flight flight)
        {
            log.Info("Starting up a new ascend spy");
            this._ascendActive = true;
            this._flight = flight;
            this._planetname = FlightGlobals.ActiveVessel.mainBody.name;
            this._threshold = PlanetThresholdManager.ThresholdForPlanet(this._planetname);

            flight.StartAscend(this._planetname, FlightLogger.met, Util.VesselHeightFromTerrain(FlightGlobals.ActiveVessel));
        }

        public void Update()
        {
            long vesselHeightFromTerrain = Util.VesselHeightFromTerrain(FlightGlobals.ActiveVessel);
            log.DebugFormat("Adding a new ascend point met is {0} height is {1}", FlightLogger.met, vesselHeightFromTerrain);
            this._flight.AddAscendPoint(vesselHeightFromTerrain, FlightLogger.met);
        }

        public bool ShouldFlightSpyStop()
        {
            return Util.VesselHeightFromTerrain(FlightGlobals.ActiveVessel) >= _threshold;
        }

        public void StopFlightSpy()
        {
            long vesselHeight = Util.VesselHeightFromTerrain(FlightGlobals.ActiveVessel);
            log.DebugFormat("Stopping the Ascend spy, vesselheight is {0}, threshold is {1}", vesselHeight, this._threshold);

            bool ascendDone = vesselHeight >= this._threshold;

            this._flight.StopAscend(ascendDone);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb = sb.Append("Ascend spy that is currently ").Append(this._ascendActive ? "active" : "inactive");
            return sb.ToString();
        }
    }
}

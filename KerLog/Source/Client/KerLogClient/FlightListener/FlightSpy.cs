using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KerLogClient.FlightsManager;
using KerLogClient.General;
using KerLogData.FlightData;
using UnityEngine;

namespace KerLogClient.FlightListener
{
    class FlightSpy
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static FlightSpy _flightSpy;
        private FlightSpyVelocity _deltaVSpy;
        private Flight _flight;
        private bool _ascendActive;
        private double _metAtStart;
        private double _ascendMetStore;

        private double _deltaVMetStore;

        private FlightSpy()
        {
            FlightSpyStarter.FlightStart += FlightSpyStarter_FlightStart;
            FlightSpyStarter.FlightUpdate += FlightSpyStarter_FlightUpdate;
            FlightSpyStarter.FlightStop += FlightSpyStarter_FlightStop;
            log.Info("FlightSpy subscribed to all FlightSpyStarter events");
        }

        void FlightSpyStarter_FlightStop()
        {
            if (_ascendActive)
            {
                if (AscendIsStopping)
                {
                    FireOnAscendStop(true);
                }
                else
                {
                    FireOnAscendStop(false);
                }
            }
            Manager.PersistFlightToSandbox(_flight);
        }

        void FlightSpyStarter_FlightUpdate(int dTimeMilSeconds)
        {
            _deltaVMetStore += dTimeMilSeconds;

            if(_deltaVMetStore >= 1000)
            {
                FireDeltaVUpdateEvent(FlightLogger.met, FlightGlobals.ActiveVessel.horizontalSrfSpeed);
                _deltaVMetStore -= 1000;
            }

            if (!_ascendActive && AscendIsStarting)
            {
                log.Info("Detecting a new Ascend");
                FireOnAscendStart(this, FlightLogger.met);
                _ascendActive = true;
                _ascendMetStore = 0;
            }

            if (_ascendActive)
            {
                _ascendMetStore += dTimeMilSeconds;
                if (_ascendMetStore > 1000)
                {
                    log.Debug("A second passed, firing another AscendUpdate");
                    _ascendMetStore -= 1000;
                    FireOnAscendUpdate(Util.VesselHeightFromTerrain(FlightGlobals.ActiveVessel), FlightLogger.met);
                }
            }

            if (_ascendActive && AscendIsStopping)
            {
                log.Info(string.Format("Detecting the end of an ascend, vessel at the following height {0} and planet is {1}", Util.VesselHeightFromTerrain(FlightGlobals.ActiveVessel), FlightGlobals.ActiveVessel.mainBody.name));
                FireOnAscendStop(true);
                _ascendActive = false;
            }
        }

        void FlightSpyStarter_FlightStart()
        {
            _metAtStart = FlightLogger.met;
            string vesselName = FlightGlobals.ActiveVessel.name;
            log.Info(string.Format("Creating a new flight with name {0} and type {1}", vesselName, FlightGlobals.ActiveVessel.protoVessel.vesselType));
            _flight = new Flight(vesselName, FlightGlobals.ActiveVessel.protoVessel.vesselType.ToString());
            FlightSpyAscend.PrepareFlightSpyAscend();
            _deltaVSpy = new FlightSpyVelocity(_flight);
            _deltaVMetStore = 0;
        }

        public Flight Flight { get { return _flight; } }

        #region Ascend

        public bool AscendIsStarting
        {
            get
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
        }

        public bool AscendIsStopping
        {
            get
            {
                int threshold = PlanetThresholdManager.ThresholdForPlanet(FlightGlobals.ActiveVessel.mainBody.name);
                log.Debug(string.Format("Threshold is {0} and current height is {1} ", threshold, Util.VesselHeightFromTerrain(FlightGlobals.ActiveVessel)));
                if (_ascendActive && Util.VesselHeightFromTerrain(FlightGlobals.ActiveVessel) > threshold)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        public static void PrepareFlightSpy()
        {
            _flightSpy = new FlightSpy();
        }

        #region events

        public void FireOnAscendStart(FlightSpy flightSpy, double metStart)
        {
            if (AscendStartEvent != null)
            {
                log.Debug("Firing AscendStartEvent");
                AscendStartEvent(flightSpy, metStart);
            }
        }

        public void FireOnAscendUpdate(int vesselHeight, double currentMet)
        {
            if (AscendUpdateEvent != null)
            {
                log.Debug(string.Format("Firing AscendUpdate with a vesselheight of {0} and met of {1}", vesselHeight, currentMet));
                AscendUpdateEvent(vesselHeight, currentMet);
            }
        }

        public void FireOnAscendStop(bool ascendSucceeded)
        {
            if (AscendStopEvent != null)
            {
                log.Debug("Firing on AscendStop");
                AscendStopEvent(ascendSucceeded);
            }
        }

        public void FireDeltaVUpdateEvent(double met, double velocity)
        {
            if(DeltaVUpdateEvent != null)
            {
                DeltaVUpdateEvent(met, velocity);
            }
        }

        public delegate void AscendStart(FlightSpy flightSpy, double metStart);
        public delegate void AscendUpdate(int vesselHeight, double currentMet);
        public delegate void AscendStop(bool ascendSucceeded);

        public delegate void DeltaVUpdate(double met, double velocity);

        public static event AscendStart AscendStartEvent;
        public static event AscendUpdate AscendUpdateEvent;
        public static event AscendStop AscendStopEvent;

        public static event DeltaVUpdate DeltaVUpdateEvent;

        #endregion
    }
}

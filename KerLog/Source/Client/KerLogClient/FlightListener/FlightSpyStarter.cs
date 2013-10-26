using System;
using UnityEngine;

namespace KerLogClient.FlightListener
{
    /// <summary>
    /// Handles the logic for starting and saving a flightspy (one per vessel)
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class FlightSpyStarter : MonoBehaviour
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Vessel _vessel;
        private double _lastMet;
        private bool _startedFlight = false;

        FlightSpyManager _manager;

        #region MonoBehaviour

        private void Start()
        {
            log.Debug("Setting up the FlightSpyStarter");
            _vessel = FlightGlobals.ActiveVessel;
            _lastMet = FlightLogger.met;
            _manager = FlightSpyManager.SpyManager;
        }

        private void Update()
        {
            if (FlightLogger.met > 0 && !_startedFlight && !FlightGlobals.ActiveVessel.isEVA)
            {
                log.Info(string.Format("Detecting a start of flight that is not an EVA"));
                FireOnFlightStart();
                _startedFlight = true;
            }
            else if(FlightGlobals.ActiveVessel.isEVA)
            {
                log.Info("Detecting a start of flight that is an EVA");
                _startedFlight = true;
            }

            if (_vessel != FlightGlobals.ActiveVessel)
            {
                log.Info(string.Format("Vessel change detected, from vessel {0} to vessel {1}", _vessel.name, FlightGlobals.ActiveVessel.name));
                FireOnFlightStop();
                _startedFlight = false;
                _vessel = FlightGlobals.ActiveVessel;
            }
            else if (_startedFlight && !FlightGlobals.ActiveVessel.isEVA)
            {
                FireOnUpdate(DTimeLastUpdate(FlightLogger.met - _lastMet));
            }
            _lastMet = FlightLogger.met;
        }

        private void OnDestroy()
        {
            log.Debug("Firing FlightStopEvent");
            FireOnFlightStop();
        }

        #endregion

        #region Private Methods

        private int DTimeLastUpdate(double dInSeconds)
        {
            return (int)(dInSeconds * 1000);
        }

        #endregion

        #region Events

        private void FireOnFlightStart()
        {
            if (FlightStart != null)
            {
                log.Debug("Firing FlightStart event");
                FlightStart();
            }
        }

        private void FireOnFlightStop()
        {
            if (FlightStop != null)
            {
                log.Debug("Firing FlightStop event");
                FlightStop();
            }
        }

        private void FireOnUpdate(int dTime)
        {
            if (FlightUpdate != null)
            {
                log.Debug(string.Format("Firing Update event with a dTime of {0}", dTime));
                FlightUpdate(dTime);
            }
        }

        public delegate void KSPClientEvent();
        public delegate void KSPVesselChangedEvent(Vessel newVessel);
        public delegate void KSPUpdate(int dTimeMilSeconds);

        public static event KSPClientEvent FlightStart;
        public static event KSPUpdate FlightUpdate;
        [Obsolete("Not used, FlightStop --> FlightStart is used on a vessel change")]
        public static event KSPVesselChangedEvent VesselChanged;
        public static event KSPClientEvent FlightStop;

        #endregion
    }
}

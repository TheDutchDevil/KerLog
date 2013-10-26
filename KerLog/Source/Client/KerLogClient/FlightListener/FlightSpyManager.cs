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
    class FlightSpyManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static FlightSpyManager _flightSpy;
        private Flight _flight;
        private double _metAtStart;

        private List<IFlightSpy> _registeredFlightSpies;

        /// <summary>
        /// List with tuples, the first int is the delta time
        /// in met since the ShouldSpyBeStarted method was 
        /// called
        /// </summary>
        private List<Tuple<double, IFlightSpy>> _validFlightSpies;

        private List<Tuple<double, IFlightSpy>> _activeFlightSpies;

        private FlightSpyManager()
        {
            FlightSpyStarter.FlightStart += FlightSpyStarter_FlightStart;
            FlightSpyStarter.FlightUpdate += FlightSpyStarter_FlightUpdate;
            FlightSpyStarter.FlightStop += FlightSpyStarter_FlightStop;
            log.Info("FlightSpy subscribed to all FlightSpyStarter events");

            this._validFlightSpies = new List<Tuple<double, IFlightSpy>>();
            this._registeredFlightSpies = new List<IFlightSpy>();
            this._activeFlightSpies = new List<Tuple<double, IFlightSpy>>();
        }

        public void AddFlightSpy(IFlightSpy spy)
        {
            log.DebugFormat("Adding a flight spy to the list of registered flight spies. '{0}'", spy.ToString());
            this._registeredFlightSpies.Add(spy);
        }

        void FlightSpyStarter_FlightStop()
        {
            log.InfoFormat("Flight is stopping, removing all the valid and active flight spies");

            this._activeFlightSpies.RemoveRange(0, this._activeFlightSpies.Count);
            this._validFlightSpies.RemoveRange(0, this._validFlightSpies.Count);

            Manager.PersistFlightToSandbox(_flight);
        }

        void FlightSpyStarter_FlightUpdate(int dTimeMilSeconds)
        {
            double dTimeSeconds = (double)dTimeMilSeconds / 1000d;

            List<Tuple<double, IFlightSpy>> startingFlightSpies = new List<Tuple<double, IFlightSpy>>();

            for (int i = 0; i < this._validFlightSpies.Count; i++)
            {
                this._validFlightSpies[i].Item1 += dTimeSeconds;
                double dTimeBetweenUpdate = this._validFlightSpies[i].Item1;
                IFlightSpy flightSpy = this._validFlightSpies[i].Item2;

                if(dTimeBetweenUpdate > flightSpy.TimeBetweenUpdate)
                {
                    log.DebugFormat("Checking if flight spy '{0} should be started since the dTime threshold elapsed", flightSpy.ToString());
                    this._validFlightSpies[i].Item1 -= flightSpy.TimeBetweenUpdate;

                    if(flightSpy.ShouldStartFlightSpy())
                    {
                        log.InfoFormat("Starting the following flight spy since the conditions are met '{0}'", flightSpy.ToString());
                        flightSpy.StartFlightSpy(this._flight);
                        startingFlightSpies.Add(this._validFlightSpies[i]);
                    }
                }
            }

            foreach(Tuple<double, IFlightSpy> tuple in startingFlightSpies)
            {
                this._validFlightSpies.Remove(tuple);
                tuple.Item1 -= dTimeSeconds;
                tuple.Item2.Update();
                this._activeFlightSpies.Add(tuple);
            }

            List<Tuple<double, IFlightSpy>> stoppingFlightSpies = new List<Tuple<double, IFlightSpy>>();

            for (int i = 0; i < this._activeFlightSpies.Count; i++)
            {
                this._activeFlightSpies[i].Item1 += dTimeSeconds;
                double dTimeBetweenUpdate = this._activeFlightSpies[i].Item1;
                IFlightSpy flightSpy = this._activeFlightSpies[i].Item2;

                if(dTimeBetweenUpdate > flightSpy.TimeBetweenUpdate)
                {
                    log.DebugFormat("Time between updates elapsed for flight spy '{0}'", flightSpy);

                    this._activeFlightSpies[i].Item1 -= flightSpy.TimeBetweenUpdate;
                    flightSpy.Update();

                    if(flightSpy.ShouldFlightSpyStop())
                    {                        
                        stoppingFlightSpies.Add(this._activeFlightSpies[i]);
                    }
                }
            }

            foreach (Tuple<double, IFlightSpy> tuple in stoppingFlightSpies)
            {
                log.InfoFormat("Stopping flight spy '{0}'", tuple.Item2.ToString());
                tuple.Item2.StopFlightSpy();
                this._activeFlightSpies.Remove(tuple);

                if(tuple.Item2.PreRequisitesAreMet(_flight))
                {
                    log.InfoFormat("Flight spy '{0}' still has its pre requisites met, adding it to the list of valid flight spies", tuple.Item2.ToString());
                    this._validFlightSpies.Add(tuple);
                }
            }
        }

        void FlightSpyStarter_FlightStart()
        {
            _metAtStart = FlightLogger.met;
            string vesselName = FlightGlobals.ActiveVessel.name;
            log.Info(string.Format("Creating a new flight with name {0} and type {1}", vesselName, FlightGlobals.ActiveVessel.protoVessel.vesselType));
            _flight = new Flight(vesselName, FlightGlobals.ActiveVessel.protoVessel.vesselType.ToString());

            log.Debug("Looping through the flight spies for their preReqs");

            for (int i = 0; i < this._registeredFlightSpies.Count; i++)
            {
                if(this._registeredFlightSpies[i].PreRequisitesAreMet(_flight))
                {
                    IFlightSpy flightSpy = this._registeredFlightSpies[i];

                    if(flightSpy.ShouldStartFlightSpy())
                    {
                        log.InfoFormat("Flight spy '{0}' met pre reqs and met the conditions for starting it. Activating it and adding it to the list of active flight spies", flightSpy.ToString());
                        flightSpy.StartFlightSpy(this._flight);
                        flightSpy.Update();
                        this._activeFlightSpies.Add(new Tuple<double, IFlightSpy>(0, flightSpy));
                    }
                    else
                    {
                        log.InfoFormat("Flight spy '{0}' met pre reqs, adding it to the list of valid flight spies", flightSpy.ToString());
                        this._validFlightSpies.Add(new Tuple<double, IFlightSpy>(0, flightSpy));
                    }
                }
            }
        }      

        private static FlightSpyManager PrepareFlightSpy()
        {
           return new FlightSpyManager();
        }

        public static FlightSpyManager SpyManager
        {
            get
            {
                if(_flightSpy == null)
                {
                    _flightSpy = PrepareFlightSpy();
                }
                return _flightSpy;
            }
        }
    }
}

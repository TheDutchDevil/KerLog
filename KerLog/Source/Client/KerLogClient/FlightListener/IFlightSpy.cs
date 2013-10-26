using KerLogData.FlightData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerLogClient.FlightListener
{
    /// <summary>
    /// Interface that defines the behaviour of FlightSpies used 
    /// by the FlightSpyManager
    /// </summary>
    interface IFlightSpy
    {
        /// <summary>
        /// The time in seconds of mission elapsed time that should
        /// pass between a call to the update method. 
        /// </summary>
        double TimeBetweenUpdate { get; }

        /// <summary>
        /// Function that is called once to determine that the 
        /// FlightSpy is in a valid state. If the function returns
        /// false the FlightSpy will not be started or called by the
        /// FlightSpyManager
        /// </summary>
        /// <returns>True if all the pre requisites for starting the
        /// FlightSpy are met. False if they are not met.</returns>
        bool PreRequisitesAreMet(Flight flight);

        /// <summary>
        /// Function called by the flight spy manager to determine
        /// whether the Flight spy should be started. If the method
        /// returns true it will not be called again until the 
        /// StopFlightSpy method returns true.
        /// </summary>
        /// <returns>True if the conditions for starting the FlightSpy
        /// are met</returns>
        bool ShouldStartFlightSpy();

        /// <summary>
        /// Function called by the flight spy manager after which the
        /// IFlightSpy should set up its internal resources to start
        /// receiving updates
        /// </summary>
        void StartFlightSpy(Flight flight);

        /// <summary>
        /// Method that is called for the specified MET interval after
        /// the start flight spy method has returned true
        /// </summary>
        void Update();

        /// <summary>
        /// Method called by the flight spy manager to determine whether
        /// the pre requisites for stopping the flight spy manager are met
        /// If the method returns true the flight spy manager will stop calling
        /// the update method and start calling the StartFlightSpy method again
        /// </summary>
        /// <returns></returns>
        bool ShouldFlightSpyStop();

        /// <summary>
        /// Sends a stop signal to the flight spy after which it shuts
        /// down and releases any resources used.
        /// </summary>
        void StopFlightSpy();

        string ToString();
    }
}

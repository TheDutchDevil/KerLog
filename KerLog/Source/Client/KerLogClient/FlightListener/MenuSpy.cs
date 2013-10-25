using KerLogClient.FlightsManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerLogClient.FlightListener
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    class MenuSpy : MonoBehaviour
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private void Start()
        {
            log.Info("Attempting to send flights to database");
            Manager.AttemptToSendFlights();
        }
    }
}

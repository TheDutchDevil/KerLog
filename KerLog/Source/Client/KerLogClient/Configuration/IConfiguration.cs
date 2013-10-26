using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerLogClient.Configuration
{
    /// <summary>
    /// Element representing a configuration
    /// </summary>
    interface IConfiguration
    {
        List<string> PersistedFlights { get; }

        void AddPersistedFlight(string path);

        void RemovePersistedFlight(string path);

        string IP { get; }

        int Port { get; }
    }
}

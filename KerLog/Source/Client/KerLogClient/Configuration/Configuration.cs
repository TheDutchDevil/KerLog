using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using KSP.IO;

namespace KerLogClient.Configuration
{
   /// <summary>
   /// Implementation of IConfiguration, offered by the Configuration
   /// Provider. Implements IPropertyChanged and immediately saves a 
   /// configuration value after it is set.
   /// </summary>
    class Configuration : IConfiguration, INotifyPropertyChanged
    {
        private object _persistedFlightsLock;

        private PluginConfiguration _plugConf;

        private List<string> _persistedFlights;

        private string _iP;

        private int _port;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Configuration()
        {
            _plugConf = PluginConfiguration.CreateForType<Configuration>();
            _plugConf.load();
            PropertyChanged += Configuration_PropertyChanged;
            string rawPersistedFlights = _plugConf.GetValue<string>("persistedFlights");
            _iP = _plugConf.GetValue<string>("iP");
            _port = _plugConf.GetValue<int>("port",-1);
            _persistedFlightsLock = new object();

            if(_port == -1)
            {
                log.Debug("Port was not found in the configuration");
                _port = 10000;
            }

            if (_iP == null)
            {
                log.Debug("IP was not found in the configuration");
                IP = "192.168.1.7";
            }
            if (rawPersistedFlights == null)
            {
                log.Debug("Persisted flights was not found in the configuration");
                PersistedFlights = new List<string>();
            }
            else
            {
                try
                {
                    _persistedFlights = rawPersistedFlights.Split('|').Reverse().ToList<string>();
                }
                catch (Exception ex)
                {
                    log.Info(string.Format("Could not format persisted flights into an array, resetting it to default value. Persistedflights value is {0}", rawPersistedFlights), ex);
                    PersistedFlights = new List<string>();
                }
            }
        }

        void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("_persistedFlights"))
            {
                _plugConf.SetValue("persistedFlights", string.Join("|", _persistedFlights.ToArray()));
            }
            else if (e.PropertyName.Equals("_iP"))
            {
                _plugConf.SetValue("iP", _iP);
            }
            else if(e.PropertyName.Equals("_port"))
            {
                _plugConf.SetValue("port", _port);
            }
            else
            {
                log.Warn(string.Format("Property changed {0} was fired but it is not supported", e.PropertyName));
            }
            Save();
        }

        public void Save()
        {
            log.Debug("Saving configuration");
            _plugConf.save();
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                log.Debug(string.Format("Firing OnPropertyChanged for property {0}", name));
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public List<string> PersistedFlights
        {
            get
            {
                return _persistedFlights;
            }
            private set
            {
                lock (_persistedFlightsLock)
                {
                    _persistedFlights = value;
                    OnPropertyChanged("_persistedFlights");
                }
            }
        }


        public void AddPersistedFlight(string path)
        {
            lock (_persistedFlightsLock)
            {
                if (_persistedFlights.Contains(path))
                {
                    log.Debug(string.Format("Persisted flights already contains {0}", path));
                    return;
                }
                log.Debug(string.Format("Adding {0} to persistedFlights", path));
                _persistedFlights.Add(path);
                OnPropertyChanged("_persistedFlights");
            }
        }

        public void RemovePersistedFlight(string path)
        {
            lock (_persistedFlightsLock)
            {
                if (!_persistedFlights.Contains(path))
                {
                    log.Debug(string.Format("Persisted flights does not contain {0}", path));
                    return;
                }
                log.Debug(string.Format("Removing {0} from persistedFlights", path));
                _persistedFlights.Remove(path);
                OnPropertyChanged("_persistedFlights");
            }
        }


        public string IP
        {
            get { return _iP; }
            private set
            {
                _iP = value;
                OnPropertyChanged("_iP");
            }
        }


        public int Port
        {
            get { return _port; }
            private set
            {
                _port = value;
                OnPropertyChanged("_port");
            }
        }
    }
}

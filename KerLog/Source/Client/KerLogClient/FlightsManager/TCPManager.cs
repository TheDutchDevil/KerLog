using KerLogClient.Configuration;
using KerLogClient.General;
using KerLogData.FlightData;
using KerLogData.FlightManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Threading;

namespace KerLogClient.FlightsManager
{
    class TCPManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string _ip;
        private static int _port;
        private static Thread t;

        public static void SendFlightsList(List<Flight> flights)
        {

            _ip = ConfigurationProvider.Configuration.IP;
            _port = ConfigurationProvider.Configuration.Port;

            if (flights == null || flights.Count == 0)
            {
                return;
            }
        
            log.Info("Creating a new thread to persist a new list of flights");
            t = new Thread(new ParameterizedThreadStart(SendFlightsListAsync));
            t.Start(flights);
        }


        public static void SendFlightsListAsync(object flightListObj)
        {
            List<Flight> flights = flightListObj as List<Flight>;
            if (flights == null)
            {
                log.Error("Flights list passed to send async is invalid");
                return; ;
            } else if(flights.Count == 0)
            {
                log.Warn("No flights to send");
                return;
            }

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                log.Debug(string.Format("Attempting to connect to remote server {0}:{1}", ConfigurationProvider.Configuration.IP, ConfigurationProvider.Configuration.Port));

                try
                {
                    socket.Connect(Dns.GetHostEntry(ConfigurationProvider.Configuration.IP).AddressList, ConfigurationProvider.Configuration.Port);
                }
                catch (SocketException ex)
                {
                    log.Info(string.Format("Could not connect to the remote server at {0}:{1}. {2}", ConfigurationProvider.Configuration.IP, ConfigurationProvider.Configuration.Port, ex.Message));
                    return;
                }

                using(NetworkStream stream = new NetworkStream(socket))
                {
                    log.Debug(string.Format("Managed to connect to the server, waiting to see if I can stay connect"));

                    if (!StreamUtil.ReadObjectFromStream<bool>(stream))
                    {
                        log.Info(string.Format("Server aborted the connection for the following reason '{0}'", StreamUtil.ReadObjectFromStream<string>(stream)));
                        socket.Disconnect(false);
                        return;
                    }

                    StreamUtil.WriteToStream(Util.UniqueHashID(), stream);

                    bool canSendFlights = StreamUtil.ReadObjectFromStream<bool>(stream);

                    log.Debug(string.Format("I {0} send flights to the server", canSendFlights ? "can" : "cannot"));
                    if(!canSendFlights)
                    {
                        socket.Disconnect(false);
                        return;
                    }

                    foreach(Flight f in flights)
                    {
                        log.Debug(string.Format("Attempting to send flight {0} to server", f.VesselName));
                        StreamUtil.WriteToStream(true, stream);

                        StreamUtil.ReadObjectFromStream<object>(stream);

                        log.Debug(string.Format("Received answer from the server, actually sending flight"));

                        StreamUtil.WriteToStream(f, stream);

                        if (StreamUtil.ReadObjectFromStream<bool>(stream))
                        {
                            log.Debug("Flight succesfully persisted, removing from sandbox");
                            Manager.RemoveFlightFromSandbox(f);
                        }
                        else
                        {
                            log.Warn("Flight was not succesfully saved on the server, not removing from sandbox");
                        }
                    }

                    log.Debug("All flights sent, closing connection");
                    StreamUtil.WriteToStream(false, stream);

                    socket.Disconnect(false);
                    log.Debug("Disconnected from socket");
                }
            }
            catch(SocketException sockEx)
            {
                log.Error(string.Format("An error occured while connecting to {0}:{1}", ConfigurationProvider.Configuration.IP, ConfigurationProvider.Configuration.Port), sockEx);
                return;
            }
        }
    }
}

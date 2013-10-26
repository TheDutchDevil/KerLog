using KerLogData.FlightData;
using KerLogData.FlightManager;
using NetworkCommsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientListener.KerLogListener
{
    class Listener
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _port;
        private bool _runServer;
        private Socket _listener;
        private ThreadManager _threadManager;
     
        public Listener(int port)
        {
            _port = port;
            _threadManager = new ThreadManager(10);
        }

        public void Start()
        {
            _runServer = true;
            log.Info("Starting a new thread to listen to requests");
            Thread t = new Thread(ListenAsync);
            t.Start();
        }

        private void ListenAsync()
        {
            IPAddress[] ipv4Addresses = Array.FindAll(
                Dns.GetHostEntry(string.Empty).AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            IPAddress ipAddress = ipv4Addresses[0];
            log.Debug(string.Format("Trying to listen to {0}:{1}", ipAddress, _port));
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _port);

            Socket _listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                _listener.Bind(new IPEndPoint(IPAddress.Any, _port));
                _listener.Listen(200);

                while (_runServer)
                {
                    AcceptClient(_listener.Accept());
                }
            }
            catch (Exception e)
            {
                log.Error("Exception occured while listening for connection", e);
            }
        }

        private void AcceptClient(Socket client)
        {
            log.Info(string.Format("New client connected! IP is {0}", client.RemoteEndPoint));

            Thread newThread = _threadManager.CreateNewThread(new ParameterizedThreadStart(StartConnectionListening));

            if(newThread != null)
            {
                newThread.Start(client);
            }
            else
            {
                // TODO: Split to killablethread
                log.Info(string.Format("Not enought capacity to deal with the client at {0}", client.RemoteEndPoint));
                using(NetworkStream stream = new NetworkStream(client))
                {
                    StreamUtil.WriteToStream(false, stream);
                    StreamUtil.WriteToStream("Connection aborted because server is too busy", stream);
                }
                client.Disconnect(false);
            }            
        }

        public void StartConnectionListening(object state)
        {
            Socket handler = state as Socket;

            try
            {
                using (var stream = new NetworkStream(handler))
                {
                    StreamUtil.WriteToStream(true, stream);

                    string hash = StreamUtil.ReadObjectFromStream<string>(stream);

                    log.Debug(string.Format("Received hash {0}", hash));

                    DatabaseManager dbsManager = new DatabaseManager(hash);

                    if (dbsManager.ConnectionIsValid)
                    {
                        log.Debug("Connection is valid, asking for flights");
                        StreamUtil.WriteToStream(true, stream);

                        bool hasFlights = StreamUtil.ReadObjectFromStream<bool>(stream);
                        while (hasFlights)
                        {
                            log.Debug("Flights available, asking for a single flight");
                            StreamUtil.WriteToStream(new object(), stream);
                            Flight f = StreamUtil.ReadObjectFromStream<Flight>(stream);

                            log.DebugFormat("Writing flight for vessel {0} to database and sending result to client", f.VesselName);

                            StreamUtil.WriteToStream(dbsManager.PersistFlight(f), stream);
                            hasFlights = StreamUtil.ReadObjectFromStream<bool>(stream);
                        }
                        log.Debug(string.Format("No more flights left, disconnecting socket {0}", handler.RemoteEndPoint));
                        handler.Disconnect(true);
                    }
                    else
                    {
                        log.Warn(string.Format("DB for hash {0} is invalid", hash));
                        StreamUtil.WriteToStream(false, stream);
                        handler.Disconnect(true);
                    }
                }
            }
            catch (SerializationException ex)
            {
                log.Error("Error occurred when deserializing a message received by a client", ex);
                handler.Disconnect(true);
            }
        }
        public void Stop()
        {
            log.Info("Stopping with listening");
            _runServer = false;
            _listener.Disconnect(false);
        }


        // TODO: Logging / make flexible
        [Obsolete("Depends on unused network comms library")]
        private  void ReceiveOpenMessage(PacketHeader header, Connection connection, string hash)
        {
            log.Info(string.Format("Received a new client, from {0}", connection.ConnectionInfo.RemoteEndPoint));
            DatabaseManager dbsManager = new DatabaseManager("127.0.0.1", "KerLogTest", "KerLogTest", "insert", hash);

            if(dbsManager.ConnectionIsValid)
            {
                log.Debug("Connection is valid, sending waiting to see if flights are available");
                bool hasFlightsToSend = connection.SendReceiveObject<bool>("connectionResult", "hasFlightsToSend", 3000, true);
                while(hasFlightsToSend)
                {
                    log.Debug("Flight available, waiting for the flight");
                    Flight flight = connection.SendReceiveObject<Flight>("requestFlight", "Flight", 3000);
                    log.Debug(string.Format("Flight available, vessel name is {0}", flight.VesselName));
                    hasFlightsToSend = connection.SendReceiveObject<bool>("persistResult", "hasFlightsToSend", 3000, dbsManager.PersistFlight(flight));
                }
                log.Debug("No more flights available, closing connection");
                connection.CloseConnection(false);
            }
            else
            {
                log.Info("database connection could not be made, closing connection");
                connection.SendObject("connectionResult", false);
                connection.CloseConnection(false);
            }
        }
    }
}

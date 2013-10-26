using KerLogData.FlightData;
using KerLogData.FlightManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientListener.KerLogListener
{
    class DatabaseManager
    {
        private readonly string _dbsIP;
        private readonly string _dbs;
        private readonly string _username;
        private readonly string _password;
        private readonly string _connString;
        private readonly string _hash;

        public const string IP = "127.0.0.1";
        public const string DATABASE = "KerLogTest";
        public const string USERNAME = "KerLogTest";
        public const string PASSWORD = "insert";

        private int _id;

        private SqlConnection _connection;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DatabaseManager(string hash) : this(IP, DATABASE, USERNAME, PASSWORD, hash)
        {

        }

        public DatabaseManager(string dbsIP, string dbs, string username, string password, string hash)
        {
            this._dbsIP = dbsIP;
            this._dbs = dbs;
            this._username = username;
            this._password = password;
            this._hash = hash;            
            _connString = string.Format("Server={0};Database={1};User Id={2};Password={3};", _dbsIP, _dbs, _username, _password);
            log.Debug(string.Format("Attempting to connect to database with connection string '{0}'", _connString));
            _connection = new SqlConnection(_connString);

            _id = -1;
            SetUp();
        }

        private void SetUp()
        {
            try
            {
                _connection.Open();
            }
            catch (SqlException ex)
            {
                log.Fatal("Could not connect to MSSQL DBS", ex);
                throw;
            }
            finally
            {
                _connection.Close();
            }


            if (!DatabaseContainsHash(_hash))
            {
                InsertHash(_hash);
            }

            _id = GetID(_hash);
        }

        private bool DatabaseContainsHash(string hash)
        {
            bool exists = false;
            log.Debug(string.Format("Checking to see if database contains hash {0}", hash));
            try
            {
                _connection.Open();

                using (SqlCommand command = new SqlCommand("select ID from SenderHash where Hash = @hash", _connection))
                {
                    command.Parameters.AddWithValue("@hash", hash);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        exists = reader.HasRows;
                    }
                }
            }
            catch (SqlException exception)
            {
                log.Error("Could not get hash from database", exception);
            }
            finally
            {
                _connection.Close();
            }
            log.Debug("Has found is: " + exists);
            return exists;
        }

        private void InsertHash(string hash)
        {
            log.Debug(string.Format("Attempting to insert hash {0} into database", hash));
            try
            {
                _connection.Open();

                using (SqlCommand command = new SqlCommand("INSERT INTO SenderHash (Hash) VALUES (@hash)", _connection))
                {
                    command.Parameters.AddWithValue("@hash", hash);

                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                log.Fatal("Encountered an exception while inserting hash into database", exception);
               throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        public int GetID(string hash)
        {
            log.Debug(string.Format("Resolving ID for hash {0}", hash));
            int result = -1;
            try
            {
                _connection.Open();

                using (SqlCommand command = new SqlCommand("select ID from SenderHash where Hash = @hash", _connection))
                {
                    command.Parameters.AddWithValue("@hash", hash);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = reader.GetInt32(0);
                        }
                    }
                }
            }
            catch (SqlException exception)
            {
                log.Fatal("Encountered an exception while reading ID from database", exception);
                throw;
            }
            finally
            {
                _connection.Close();
            }
            log.Debug(string.Format("Has id is {0}", result));
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flight"></param>
        /// <returns>true if the flight has not been persisted</returns>
        private bool AttemptToPersistFlight(Flight flight)
        {
            log.Info(string.Format("Attempting to insert flight {0} into databse", flight.VesselName));
            bool failed = false;
            try
            {
                _connection.Open();

                using (SqlCommand command = new SqlCommand("INSERT INTO FlightData (InsertTimeStamp, SenderHashID, FlightData) VALUES (GetDate(), @hashID, @flight)", _connection))
                {
                    command.Parameters.AddWithValue("@hashID", _id);
                    SqlParameter param = command.Parameters.Add("@flight", SqlDbType.VarBinary);
                    param.Value = ProtoBufWrapper.ToByteArray(flight);

                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                log.Error("Something went wrong while persisting the flight", exception);
               failed = true;
            }
            finally
            {
                _connection.Close();
            }
            log.Info(string.Format("Result of attempt to persist flight operation for flight {0} is {1}", flight.VesselName, !failed));
            return failed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flight"></param>
        /// <returns>true if the flight has been persisted</returns>
        private bool StartPersistFlightTask(Flight flight)
        {

            if (flight == null)
            {
                log.Warn("A null value flight was passed into the persist flight method");
                return false ;
            }
            else if (!AttemptToPersistFlight(flight))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flight"></param>
        /// <returns>true if the flight has been persisted</returns>
        public bool PersistFlight(Flight flight)
        {
            if (!ConnectionIsValid)
            {
                log.Warn(string.Format("Connection invalid, cannot persist flight {0}", flight.VesselName));
                return false;
            }

            return StartPersistFlightTask(flight);
        }

        public string ConnectionString
        {
            get { return _connString; }
        }

        public bool ConnectionIsValid
        {
            get
            {
                return _id > -1;
            }
        }
    }
}

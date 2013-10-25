using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace KerLogData.FlightData
{
    [ProtoContract]
    public class Flight
    {
        [ProtoMember(1)]
        private DateTime _flightStart;

        [ProtoMember(2)]
        private string _vesselName;

        [ProtoMember(3)]
        private string _vesselType;

        [ProtoMember(4)]
        private List<AscendProfile> _ascendProfiles;

        [ProtoMember(5, IsRequired = false)]
        private Dictionary<double, double> _velocityForFlight;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="vesselName">The name of the vessel on the current flight</param>
        /// <param name="flightType">The vesselType, something like 'Spaceplane', 'Rocket', 'Drone'
        /// whatever is relevant</param>
        public Flight(string vesselName, string flightType)
        {
            _vesselName = vesselName;
            _vesselType = flightType;
            _flightStart = DateTime.Now;
            _ascendProfiles = new List<AscendProfile>();
            _velocityForFlight = new Dictionary<double, double>();
        }

        private Flight()
        {
        }

        [ProtoAfterDeserialization]
        public void AfterDeserialization()
        {
            if(_velocityForFlight == null)
            {
                _velocityForFlight = new Dictionary<double, double>();
            }
        }


        #region Ascend

        public void StartAscend(string planet, double metAtStart)
        {
            if(IsAnyAscendActive)
            {
                throw new InvalidOperationException("No Ascend can be added if an ascend is already active!");
            }

            _ascendProfiles.Add(new AscendProfile(planet, metAtStart));
        }

        public void AddVelocityPoint(double met, double velocity)
        {
            this._velocityForFlight.Add(met, velocity);
        }

        public bool IsAnyAscendActive { get { return _ascendProfiles.Any(ap => (ap.IsAscendActive)); } }

        public AscendProfile ActiveAscendProfile { get { return _ascendProfiles.Single(ap => (ap.IsAscendActive == true)); } }

        public DateTime FlightStart { get { return _flightStart; } }

        public void AddAscendPoint(long heightInMeters, double met)
        {
            ActiveAscendProfile.AddAscendPoint(heightInMeters, met);
        }

        public void StopAscend(bool ascendSucceeded)
        {
            ActiveAscendProfile.StopAscend(ascendSucceeded);
        }

        public string VesselName { get { return _vesselName; } }

        /// <summary>
        /// The vesselType, something like 'Spaceplane', 'Rocket', 'Drone' whatever is relevant
        /// </summary>
        public string VesselType { get { return _vesselType; } }

        #endregion
    }
}

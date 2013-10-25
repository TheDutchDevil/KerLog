using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.ComponentModel;

namespace KerLogData.FlightData
{
    [ProtoContract]
    public class AscendProfile
    {
        [ProtoMember(1)]
        private double _ascendStart;
        [ProtoMember(2)]
        private List<AscendPoint> _ascendPoints;
        [ProtoMember(3)]
        private bool _ascendActive;
        [ProtoMember(4)]
        private string _planet;

        [ProtoMember(5)]
        [DefaultValue(false)]
        private bool _ascendSucceeded;

        private AscendProfile()
        {
        }

        public AscendProfile(string planet, double metAtStart)
        {
            _ascendStart = metAtStart;
            _ascendPoints = new List<AscendPoint>();
            _ascendActive = true;
            _planet = planet;
        }

        public double AscendStart { get { return _ascendStart; } }
        public List<AscendPoint> AscendPoints { get { return _ascendPoints; } }
        public bool IsAscendActive { get { return _ascendActive; } }
        public string Planet { get { return _planet; } }
        public bool AscendSucceeded { get { return _ascendSucceeded; } }

        public void AddAscendPoint(long heightInMeters, double met)
        {
            if (!_ascendActive)
            {
                throw new InvalidOperationException("Ascend inactive");
            }

            AscendPoint point = new AscendPoint(heightInMeters);
            double deltaTime = met - _ascendStart;
            point.DeltaTSinceAscendStart = deltaTime;
        }

        public void StopAscend(bool ascendSucceeded)
        {
            _ascendSucceeded = ascendSucceeded;
            _ascendActive = false;
        }
    }
}

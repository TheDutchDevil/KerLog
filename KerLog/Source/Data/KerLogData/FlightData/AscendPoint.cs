using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace KerLogData.FlightData
{
    [ProtoContract]
    public class AscendPoint
    {
        [ProtoMember(1)]
        private long _heightInMeters;
        [ProtoMember(2)]
        private double _deltaTSinceAscendStart;

        public AscendPoint(long heightInMeters, double deltaTSinceAscendStart)
        {
            this._deltaTSinceAscendStart = deltaTSinceAscendStart;
            this._heightInMeters = heightInMeters;
        }

        private AscendPoint()
        {
        }

        public long HeightInMeters { get { return _heightInMeters; } }

        public double DeltaTSinceAscendStart { get { return _deltaTSinceAscendStart; } set { _deltaTSinceAscendStart = value; } }

    }
}

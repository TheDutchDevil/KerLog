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

        public AscendProfile(string planet, double metAtStart, long heightFromPlanet)
        {
            _ascendStart = metAtStart;
            _ascendPoints = new List<AscendPoint>();
            this.AscendPoints.Add(new AscendPoint(heightFromPlanet, 0d));
            _ascendActive = true;
            _planet = planet;
        }

        public double AscendStart { get { return _ascendStart; } }
        public List<AscendPoint> AscendPoints { get { return _ascendPoints; } }
        public bool IsAscendActive { get { return _ascendActive; } }
        public string Planet { get { return _planet; } }
        public bool AscendSucceeded { get { return _ascendSucceeded; } }

        public int FlightLength
        {
            get
            {
                return (int) this._ascendPoints.Max(ap => ap.DeltaTSinceAscendStart);
            }
        }

        /// <summary>
        /// Returns the height of the vessel at the specified time since
        /// ascend start. If no value for the time is known because the
        /// time is bigger than the total time of the ascend -1 is returned
        /// </summary>
        /// <param name="timeSinceAscendStart">The time since the start of
        /// the ascend for which the vessel height should be returned</param>
        /// <returns>The vessel height at the provided time since ascend start.
        /// -1 if no height is known for the specified time since ascend start</returns>
        public long HeightAtAscendTime(double timeSinceAscendStart)
        {
            if (timeSinceAscendStart > this.FlightLength)
            {
                return -1;
            }
            else if (timeSinceAscendStart < 0)
            {
                throw new ArgumentException("Time since ascend start can not be smaller than zero");
            }

            AscendPoint pointLower = null;

            if (timeSinceAscendStart == 0)
            {
                pointLower = this.AscendPoints.First(ascp => ascp.DeltaTSinceAscendStart == this.AscendPoints.Min(ap => ap.DeltaTSinceAscendStart));
            }
            else
            {
                pointLower = this.AscendPoints.First(ascp => ascp.DeltaTSinceAscendStart == this._ascendPoints.Where(ap => ap.DeltaTSinceAscendStart <= timeSinceAscendStart).Max(ap => ap.DeltaTSinceAscendStart));
            }

            AscendPoint pointHigher = this.AscendPoints.First(ascp => ascp.DeltaTSinceAscendStart == this._ascendPoints.Where(ap => ap.DeltaTSinceAscendStart >= timeSinceAscendStart).Min(ap => ap.DeltaTSinceAscendStart));

            double differenceFromLower = timeSinceAscendStart - pointLower.DeltaTSinceAscendStart;

            double totalDifference = pointHigher.DeltaTSinceAscendStart - pointLower.DeltaTSinceAscendStart;

            if(totalDifference == 0)
            {
                return pointLower.HeightInMeters;
            }

            int percentage = Convert.ToInt32((differenceFromLower * 100) / totalDifference);

            long totalHeightDifference = pointHigher.HeightInMeters - pointLower.HeightInMeters;

            long heightAtMet = (percentage * totalHeightDifference) / 100;

            return pointLower.HeightInMeters + heightAtMet;
        }

        public void AddAscendPoint(long heightInMeters, double met)
        {
            if (!_ascendActive)
            {
                throw new InvalidOperationException("Ascend inactive");
            }

            AscendPoint point = new AscendPoint(heightInMeters, met - _ascendStart);
            this._ascendPoints.Add(point);
        }

        public void StopAscend(bool ascendSucceeded)
        {
            _ascendSucceeded = ascendSucceeded;
            _ascendActive = false;
        }
    }
}

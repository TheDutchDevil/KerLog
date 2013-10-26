using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace KerLogVisualizer.Model.NumberCruncher.DataStorage
{
    /// <summary>
    /// List that contains ascend in a graph friendly format
    /// </summary>
    public class AscendData
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<long> _heightPoints;

        private readonly string _planetName;

        private readonly bool _areFailedAscends;

        private readonly string _chartName;

        private readonly string _chartUrl;

        public AscendData(IEnumerable<long> dataPoints, string planetName, bool failedAscends)
        {
            long highestPoint = dataPoints.Max();
            this._heightPoints = new List<long>();

            foreach(long dataPoint in dataPoints)
            {
                this._heightPoints.Add((dataPoint / highestPoint) * 100l);
            }

            this._planetName = planetName;
            this._areFailedAscends = failedAscends;

            StringBuilder sb = new StringBuilder();
            sb = sb.Append("http://chart.apis.google.com/chart?").Append("cht=lc&chs=500x500")
                .Append("&chd=t:");

            foreach(long dataPoint in _heightPoints)
            {
                sb = sb.Append(dataPoint).Append(',');
            }

            sb.Replace(',', '&', sb.Length - 1, 1);

            this._chartName = string.Format("{0} ascends on planet {1}", this._areFailedAscends ? "Failed" : "successful", this._planetName);

            sb = sb.Append("chtt=").Append(HttpUtility.UrlEncode(this._chartName));

            log.DebugFormat("Created new AscendData, url is '{0}'", this._chartUrl);
        }

        public string ChartAPIUrl
        {
            get
            {
                return _chartUrl;
            }
        }
    }
}
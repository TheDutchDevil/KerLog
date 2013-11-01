
using KerLogVisualizer.Model.NumberCruncher;
using KerLogVisualizer.Model.NumberCruncher.DataStorage;
using KerLogVisualizer.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace KerLogVisualizer
{
    public partial class Home : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SetAscendGraph();
        }

        protected void home_Init(object sender, EventArgs e)
        {
        }

        private void SetAscendGraph()
        {
            DataTable ascendData = new DataTable("Ascend data for Kerbin");
            ascendData.Columns.Add("Percentage", typeof(string));
            ascendData.Columns.Add("Height", typeof(double));

            AscendData data = AscendDataProvider.AscendDataForPlanet("Kerbin", false);

            for (int i = 0; i < data.AscendPoints.Count; i++ )
            {
                string percentage =(((double)i * 100d) / (double)data.AscendPoints.Count).ToString("0.00");

                ascendData.Rows.Add(percentage, data.AscendPoints[i]);
            }

            this.GVLineChart1.GviVAxisClass = new GoogleChartsNGraphsControls.vAxis();
            this.GVLineChart1.GviVAxisClass.Title = "Height in meters";
            this.GVLineChart1.GviVAxisClass.SlantedText = true;
            this.GVLineChart1.ChartData(ascendData);
        }
    }
}
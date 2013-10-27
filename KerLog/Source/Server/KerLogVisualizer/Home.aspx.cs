using KerLogVisualizer.Model.NumberCruncher;
using KerLogVisualizer.Model.NumberCruncher.DataStorage;
using KerLogVisualizer.Views;
using System;
using System.Collections.Generic;
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
            AscendData data = AscendDataProvider.AscendDataForPlanet("Kerbin", false);
            this.imgAscendGraph.ImageUrl = data.ChartAPIUrl;
        }

        protected void home_Init(object sender, EventArgs e)
        {
        }
    }
}
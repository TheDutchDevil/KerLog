using KerLogVisualizer.Model.NumberCruncher;
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
            AscendDataProvider.AscendDataForPlanet("Kerbin", false);
        }

        protected void home_Init(object sender, EventArgs e)
        {
        }
    }
}
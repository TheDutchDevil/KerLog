using KerLogVisualizer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace KerLogVisualizer.Views
{
    public partial class ActiveFlightAscendProfile : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            TableRow row = new TableRow();

            row.CssClass = "commandRow";

            foreach(HyperLink hp in NavigationProvider.Links)
            {
                TableCell cell = new TableCell();
                cell.Controls.Add(hp);
                cell.CssClass = "commandCell";
                row.Cells.Add(cell);
            }

            this.spCommand.Rows.Add(row);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace KerLogVisualizer.Model
{
    /// <summary>
    /// Class used by the Header usercontrol to retrieve
    /// URLs for the command bar
    /// </summary>
    public class NavigationProvider
    {
        private static Dictionary<string, string> _pages;

        private static List<HyperLink> _links;

        static NavigationProvider()
        {
            _pages = new Dictionary<string,string>();

            _pages.Add("/Home.aspx", "Home");

            _links = GetLinks();
        }

        public static Dictionary<string, string> Pages
        {
            get
            {
                return _pages;
            }
        }

        public static List<HyperLink> Links
        {
            get
            {
                return _links;
            }
        }

        private static List<HyperLink> GetLinks()
        {
            List<HyperLink> links = new List<HyperLink>();

            foreach(KeyValuePair<string, string> link in _pages)
            {
                HyperLink hyperLink = new HyperLink();

                hyperLink.NavigateUrl = link.Key;
                hyperLink.CssClass = "commandLink";
                hyperLink.Text = link.Value;

                links.Add(hyperLink);
            }

            return links;
        }
    }
}
    
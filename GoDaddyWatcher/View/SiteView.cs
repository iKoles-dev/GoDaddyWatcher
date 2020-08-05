using System;
using System.Linq;
using GoDaddyWatcher.Database;

namespace GoDaddyWatcher.View
{
    public class SiteView
    {
        public string Link { get; set; }
        public decimal Bl { get; set; }
        public decimal TrustFlow { get; set; }
        public decimal CitationFlow { get; set; }
        public string Redirects { get; set; } = "";
        public DateTime AddingTime { get; set; }

        public SiteView(Site site)
        {
            Link = site.Link;
            Bl = site.Bl;
            TrustFlow = site.TrustFlow;
            CitationFlow = site.CitationFlow;
            if (site.Redirects!=null && site.Redirects.Count > 0)
            {
                Redirects = string.Join(", ", site.Redirects.Select(x=>x.RedirectLink??""));
            }
            AddingTime = site.AddingTime;
        }
    }
}
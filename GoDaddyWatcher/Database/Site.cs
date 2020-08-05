using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GoDaddyWatcher.Database
{
    public class Site
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.None)] [Required]
        public string Link { get; set; }
        public SiteType SiteType { get; set; }
        public bool PassGoogleSearchTest { get; set; }
        public decimal Bl { get; set; }
        public decimal TrustFlow { get; set; }
        public decimal CitationFlow { get; set; }
        public List<Redirects> Redirects { get; set; } 
        public DateTime AddingTime { get; set; } = DateTime.Now;

        public void Update(Site site)
        {
            PassGoogleSearchTest = site.PassGoogleSearchTest;
            Bl = site.Bl;
            TrustFlow = site.TrustFlow;
            CitationFlow = site.CitationFlow;
            Redirects = site.Redirects;
        }

        public override string ToString()
        {
            return string.Join(" ", Link, Bl, TrustFlow, CitationFlow, string.Join(" ,",Redirects.Select(x=>x.RedirectLink)));
        }
    }
}
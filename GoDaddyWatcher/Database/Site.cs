using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public DateTime AddingTime { get; set; } = DateTime.Now;
    }
}
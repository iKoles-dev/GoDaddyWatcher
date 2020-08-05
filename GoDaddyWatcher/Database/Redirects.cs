using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoDaddyWatcher.Database
{
    public class Redirects
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string RedirectLink { get; set; }
        public string RedirectCode { get; set; }
    }
}
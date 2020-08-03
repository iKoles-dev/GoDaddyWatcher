using System.Collections.Generic;
using GoDaddyWatcher.Database;

namespace GoDaddyWatcher.Model.MainSites
{
    public abstract class Aggregator
    {
        public List<Site> Sites = new List<Site>();
        public abstract void CrawlData();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GoDaddyWatcher.Database;
using GoDaddyWatcher.Model.MainSites;
using GoDaddyWatcher.Model.MainSites.GoDaddy;

namespace GoDaddyWatcher.Model
{
    public class SiteWatcher
    {
        private Stack<Site> _sitesToCheck = new Stack<Site>();
        private readonly object _locker = new object();
        private long _threadCount;
        private int _maxThreadCount = 20;
        public SiteWatcher()
        {
            SetBaseValues();
            PeriodicalSitesCrawling();
            SitesChecking();
        }

        private void SitesChecking()
        {
            new Thread(() =>
            {
                while (true)
                {
                    lock (_locker)
                    {
                        while (_sitesToCheck.Count > 0 && _maxThreadCount > Interlocked.Read(ref _threadCount))
                        {
                            var site = _sitesToCheck.Pop();
                            Check(site);
                        }
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }){IsBackground = true}.Start();
        }

        private void Check(Site site)
        {
            Interlocked.Increment(ref _threadCount);
            new Thread(() =>
            {
                
                Interlocked.Decrement(ref _threadCount);
            }){IsBackground = true}.Start();
        }

        private void SetBaseValues()
        {
            Aggregator aggregator = new GoDaddy();
            aggregator.CrawlData();
            using (var myDbContext = new MyDbContext())
            {
                aggregator.Sites.ForEach(x =>
                {
                    x.SiteType = SiteType.Basic;
                });
                var newSites = aggregator.Sites.Where(x => myDbContext.Sites.All(z => z.Link != x.Link));
                myDbContext.AddRange(newSites);
                myDbContext.SaveChanges();
            }
        }

        private void PeriodicalSitesCrawling()
        {
            new Thread(() =>
            {
                while (true)
                {
                    Aggregator aggregator = new GoDaddy();
                    aggregator.CrawlData();
                    using (var myDbContext = new MyDbContext())
                    {
                        aggregator.Sites.ForEach(x =>
                        {
                            x.SiteType = SiteType.ToCheck;
                        });
                        var newSites = aggregator.Sites.Where(x => myDbContext.Sites.All(z => z.Link != x.Link)).ToList();
                        myDbContext.AddRange(newSites);
                        myDbContext.SaveChanges();
                        lock (_locker)
                        {
                            foreach (var newSite in newSites)
                            {
                                _sitesToCheck.Push(newSite);
                            }
                        }
                    }
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            }){IsBackground = true}.Start();
            
        }
    }
}
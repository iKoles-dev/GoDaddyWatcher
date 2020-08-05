using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using GoDaddyWatcher.Database;
using GoDaddyWatcher.Model.MainSites;
using GoDaddyWatcher.Model.MainSites.GoDaddy;
using GoDaddyWatcher.View;

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
            StatsWatcher();
            SetBaseValues();
            PeriodicalSitesCrawling();
            SitesChecking();
        }

        private void StatsWatcher()
        {
            new Thread(() =>
            {
                while (true)
                {
                    int allCount;
                    using (var db = new MyDbContext())
                    {
                        allCount = db.Sites.Count();
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ControlsContainer.Stats.Text =
                            $"Всего сайтов в базе: {allCount}\nНайдено за сеанс: {ControlsContainer.FoundSites}\nНайдено подходящих за сеанс: {ControlsContainer.FoundFittedSites}";
                        
                    });
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }){IsBackground = true}.Start();
        }

        private void SitesChecking()
        {
            new Thread(() =>
            {
                while (true)
                {
                    lock (_locker)
                    {
                        while (_sitesToCheck.Count > 0)
                        {
                            while (_maxThreadCount < Interlocked.Read(ref _threadCount))
                            {
                                Thread.Sleep(1000);
                            }
                            var site = _sitesToCheck.Pop();
                            Check(site);
                        }
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }){IsBackground = true}.Start();
        }

        private void Check(Site site)
        {
            var cachedValue = site;
            Interlocked.Increment(ref _threadCount);
            new Thread(() =>
            {
                SiteChecker siteChecker = new SiteChecker(cachedValue);
                siteChecker.Check();
                using (MyDbContext myDbContext = new MyDbContext())
                {
                    var siteInDb = myDbContext.Sites.First(x => x.Link == cachedValue.Link);
                    siteInDb.Update(cachedValue);
                    myDbContext.SaveChanges();
                }

                if (siteChecker.FitsRequirements)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ControlsContainer.GoodUsers.Add(new SiteView(cachedValue));
                    });
                    MediaPlayer mediaPlayer = new MediaPlayer();
                    mediaPlayer.Open(new Uri(Environment.CurrentDirectory + "/Resources/success.mp3", UriKind.Relative));
                    mediaPlayer.Play();
                    Interlocked.Increment(ref ControlsContainer.FoundFittedSites);
                    ResultWriter.Add(cachedValue);
                }
                Interlocked.Decrement(ref _threadCount);
            }){IsBackground = true}.Start();
        }

        private void SetBaseValues()
        {
            Aggregator aggregator = new GoDaddy();
            aggregator.CrawlData();
            using (var myDbContext = new MyDbContext())
            {
                aggregator.Sites.Distinct();
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
                        aggregator.Sites.Distinct();
                        aggregator.Sites.ForEach(x =>
                        {
                            x.SiteType = SiteType.ToCheck;
                        });
                        var newSites = aggregator.Sites.Where(x => myDbContext.Sites.All(z => z.Link != x.Link)).ToList();
                        myDbContext.AddRange(newSites);
                        myDbContext.SaveChanges();
                        ControlsContainer.FoundSites += newSites.Count;
                        lock (_locker)
                        {
                            foreach (var newSite in newSites)
                            {
                                _sitesToCheck.Push(newSite);
                            }
                        }
                    }
                }
            }){IsBackground = true}.Start();
            
        }
    }
}
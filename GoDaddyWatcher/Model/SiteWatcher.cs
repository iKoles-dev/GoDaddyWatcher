using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using GoDaddyWatcher.Database;
using GoDaddyWatcher.Model.MainSites;
using GoDaddyWatcher.Model.MainSites.GoDaddy;
using GoDaddyWatcher.View;
using Homebrew.Additional;
using Homebrew.Enums;
using Homebrew.ParserComponents;
using Newtonsoft.Json;

namespace GoDaddyWatcher.Model
{
    public class DomainsContainer
    {
        public List<string> domains;
    }
    public class SiteWatcher
    {
        private List<string> _sitesToCheck = new List<string>();
        private int _amountToCheck;
        private Stack<Site> _sitesToCheckAfterWhois = new Stack<Site>();
        private readonly object _locker = new object();
        private long _threadCount;
        private int _maxThreadCount = 200;
        private DateTime _startDate;
        public SiteWatcher()
        {
            _startDate = DateTime.Now.AddYears(-4);
            StatsWatcher();
            // SetBaseValues();
            WhoisChecking();
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

                    lock (_locker)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ControlsContainer.Stats.Text =
                                // $"Всего сайтов в базе: {allCount}\nНайдено за сеанс: {ControlsContainer.FoundSites}/ CheckTrust: {ControlsContainer.StartWebarchive} / Google {ControlsContainer.StartGoogle}\nНайдено подходящих за сеанс: {ControlsContainer.FoundFittedSites}";
                                $"Всего сайтов в базе: {allCount}\nНайдено за сеанс: {ControlsContainer.FoundSites} \\ {_amountToCheck} \\ WS: {ControlsContainer.StartWhois} \\ CT: {ControlsContainer.AllCheckTrust} \\ {ControlsContainer.EndCheckTrust} \\ WA: {ControlsContainer.EndWebarchive}\nНайдено подходящих за сеанс: {ControlsContainer.FoundFittedSites}";

                        });
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }){IsBackground = true}.Start();
        }

        private void WhoisChecking()
        {
            new Thread(() =>
            {
                while (true)
                {
                    bool hasSitesToCheck;
                    lock (_locker)
                    {
                        hasSitesToCheck = _sitesToCheck.Count > 0;
                    }

                    if (hasSitesToCheck)
                    {
                        LinkParser linkParser;
                        ReqParametres reqParametres;
                        //Проверяем доступность ПО
                        do
                        {
                            reqParametres = new ReqParametres("http://localhost:8079/greeting");
                            linkParser = new LinkParser(reqParametres.Request);
                            if (!linkParser.Data.Contains("ok\" : true"))
                            {
                                Thread.Sleep(1000);
                            }
                        } while (!linkParser.Data.Contains("ok\" : true"));
                        
                        //Отправляем данные на проверку
                        lock (_locker)
                        {
                            reqParametres = new ReqParametres("http://localhost:8079/add", HttpMethod.Post, JsonConvert.SerializeObject(new DomainsContainer{domains = _sitesToCheck}));
                            _sitesToCheck.Clear();
                        }
                        new LinkParser(reqParametres.Request);
                        
                        //Запускаем парсинг whois
                        reqParametres = new ReqParametres("http://localhost:8079/run?parser=whois");
                        new LinkParser(reqParametres.Request);
                        
                        //Проверяем ответ
                        do
                        {
                            reqParametres = new ReqParametres("http://localhost:8079/domains");
                            linkParser = new LinkParser(reqParametres.Request);
                            if (!linkParser.Data.Contains("\"scanning\" : false"))
                            {
                                Thread.Sleep(1000);
                            }
                        } while (!linkParser.Data.Contains("\"scanning\" : false"));

                        var allSites = linkParser.Data.ParsRegex("\"name\" : \"(.*?)\"(.*?)Created\" : \"(.*?)\"", 1);
                        var allDates = linkParser.Data.ParsRegex("\"name\" : \"(.*?)\"(.*?)Created\" : \"(.*?)\"", 3);
                        lock (_locker)
                        {
                                for (int i = 0; i < allSites.Count && i < allDates.Count; i++)
                                {
                                    if (DateTime.TryParse(allDates[i], out DateTime result))
                                    {
                                        if (_startDate.CompareTo(result) <= 0)
                                        {
                                            _sitesToCheckAfterWhois.Push(new Site{Link = allSites[i], SiteType = SiteType.ToCheck});
                                        }
                                        else
                                        {
                                            ControlsContainer.StartWhois++;
                                            _amountToCheck--;
                                        }
                                    }
                                    else
                                    {
                                        ControlsContainer.StartWhois++;
                                        _amountToCheck--;
                                    }
                                }
                        }
                        
                        //Очищаем данные в ПО
                        reqParametres = new ReqParametres("http://localhost:8079/clear");
                        new LinkParser(reqParametres.Request);
                    }
                    Thread.Sleep(1000);
                }
            }){IsBackground = true}.Start();
        }

        private void SitesChecking()
        {
            new Thread(() =>
            {
                while (true)
                {
                    bool hasAny;

                    lock (_locker)
                    {
                        hasAny = _sitesToCheckAfterWhois.Count > 0;
                    }

                    if (hasAny)
                    {
                        
                        while (_maxThreadCount < Interlocked.Read(ref _threadCount))
                        {
                            Thread.Sleep(1000);
                        }

                        Site site;
                        lock (_locker)
                        {
                            site = _sitesToCheckAfterWhois.Pop();
                        }
                        Check(site);
                    }
                    
                    Thread.Sleep(200);
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
                _amountToCheck--;
                using (MyDbContext myDbContext = new MyDbContext())
                {
                    var siteInDb = myDbContext.Sites.FirstOrDefault(x => x.Link == cachedValue.Link);
                    if (siteInDb == null)
                    {
                        myDbContext.Sites.Add(new Site
                            {SiteType = SiteType.ToCheck, Link = cachedValue.Link});
                        myDbContext.SaveChanges();
                        siteInDb = myDbContext.Sites.First(x => x.Link == cachedValue.Link);
                    }
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
                        var newSites = aggregator.Sites.Where(x => !myDbContext.Sites.Any(z => z.Link == x.Link)).ToList();
                        newSites.ForEach(x =>
                        {
                            x.SiteType = SiteType.ToCheck;
                        });
                        myDbContext.AddRange(newSites);
                        myDbContext.SaveChanges();
                        ControlsContainer.FoundSites += newSites.Count;
                        _amountToCheck += newSites.Count;
                        lock (_locker)
                        {
                            _sitesToCheck.AddRange(newSites.Select(x=>x.Link));
                        }
                    }
                }
            }){IsBackground = true}.Start();
            
        }
    }
}
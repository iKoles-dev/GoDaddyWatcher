using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Documents;
using GoDaddyWatcher.Database;
using GoDaddyWatcher.HomeBrew;
using GoDaddyWatcher.Model.MainSites;
using Homebrew.Additional;
using Homebrew.ParserComponents;

namespace GoDaddyWatcher.Model
{
    public class SiteChecker
    {
        private readonly Site _site;
        public bool FitsRequirements;
        public SiteChecker(Site site)
        {
            _site = site;
        }

        public void Check()
        {
            // _site.PassGoogleSearchTest = GoogleCheck();
            // if (_site.PassGoogleSearchTest)
            // {
            //     CheckTrust();
            //     if (IsFitsMinimalValues())
            //     {
            //         WebArchiveCrawl();
            //         FinalCheck();
            //     }
            // }
            
            // WebArchiveCrawl();
            // FinalCheck();
            // if (FitsRequirements)
            // {
            //     Interlocked.Increment(ref ControlsContainer.EndWebarchive);
            //     CheckTrust();
            //     if (!IsFitsMinimalValues())
            //     {
            //         FitsRequirements = false;
            //     }
            //     else
            //     {
            //         Interlocked.Increment(ref ControlsContainer.EndCheckTrust);
            //         if (!GoogleCheck())
            //         {
            //             FitsRequirements = false;
            //         }
            //     }
            // }
            
            CheckTrust();
            if (!IsFitsMinimalValues())
            {
                FitsRequirements = false;
            }
            else
            {
                Interlocked.Increment(ref ControlsContainer.EndCheckTrust);
            }

            Interlocked.Increment(ref ControlsContainer.AllCheckTrust);

            if (FitsRequirements)
            {
                WebArchiveCrawl();
                FinalCheck();
                if (FitsRequirements)
                {
                    Interlocked.Increment(ref ControlsContainer.EndWebarchive);
                    if (!GoogleCheck())
                    {
                        FitsRequirements = false;
                    }
                }
            }
            
        }

        private void FinalCheck()
        {
            if (_site.Redirects == null || _site.Redirects.Count == 0)
            {
                FitsRequirements = true;
                return;
            }

            if (_site.Redirects.All(x => x.RedirectLink.Contains(_site.Link)||x.RedirectLink.StartsWith("/")))
            {
                FitsRequirements = true;
            }
        }

        private bool IsFitsMinimalValues()
        {
            return _site.Bl>=ControlsContainer.Bl && _site.CitationFlow>=ControlsContainer.CitationFlow && _site.TrustFlow>=ControlsContainer.TrustFlow;
        }

        private bool GoogleCheck()
        {
            return true;
            LinkParser linkParser;
            // do
            // {
            //     ReqParametres reqParametres = new ReqParametres("https://www.google.com/search?q=site:"+_site.Link);
            //     reqParametres.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.38 Safari/537.36");
            //     reqParametres.SetProxy(Proxies.GetProxy());
            //     linkParser = new LinkParser(reqParametres.Request);
            //     if (linkParser.IsError || linkParser.Data.Contains("recaptcha"))
            //     {
            //         Thread.Sleep(2000);
            //     }
            // } while (linkParser.IsError|| linkParser.Data.Contains("recaptcha"));
            ReqParametres reqParametres = new ReqParametres("https://www.google.com/search?q=site:"+_site.Link);
            reqParametres.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.38 Safari/537.36");
            // reqParametres.SetProxy(Proxies.GetProxy());
            linkParser = new LinkParser(reqParametres.Request);
            return linkParser.Data.Contains("result-stats\">");
        }
        private void CheckTrust()
        {
            LinkParser linkParser;
            do
            {
                ReqParametres reqParametres = new ReqParametres($"https://checktrust.ru/app.php?r=host/app/summary/basic&applicationKey=619f496e644ba8edcd702704176a7b26&host={_site.Link}&parameterList=spam,mjDin,mjCF,mjTF");
                linkParser = new LinkParser(reqParametres.Request);
                try
                {
                    _site.Bl = decimal.Parse(linkParser.Data.ParsFromTo("mjDin\":\"", "\""), CultureInfo.InvariantCulture);
                    _site.TrustFlow = decimal.Parse(linkParser.Data.ParsFromTo("mjTF\":\"", "\""), CultureInfo.InvariantCulture);
                    _site.CitationFlow = decimal.Parse(linkParser.Data.ParsFromTo("mjCF\":\"", "\""), CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (linkParser.Data.Contains("message\":\"Host "))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            } while (linkParser.Data.Contains("message\":\"Host "));
        }

        private long _threadCount;
        private void WebArchiveCrawl()
        {
            ReqParametres reqParametres = new ReqParametres($"https://web.archive.org/cdx/search/cdx?url={_site.Link}&filter=statuscode:30.&output=json&fl=timestamp,original,digest&collapse=length");
            LinkParser linkParser = new LinkParser(reqParametres.Request);
            var timeStamps = linkParser.Data.ParsRegex("\\[\"([0-9]+)\"",1);
            List<Redirects> redirects = new List<Redirects>();
            object locker = new object();
            timeStamps.ForEach(timestamp =>
            {
                do
                {
                    reqParametres = new ReqParametres($"http://web.archive.org/web/{timestamp}/{_site.Link}");
                    linkParser = new LinkParser(reqParametres.Request);
                    string redirect = linkParser.Data.ParsFromTo("Got an HTTP ", " ");
                    string redirectUrl = linkParser.Data.ParsFromTo("<p class=\"code\">Redirecting to...</p>","<p class=\"impatient\">").ParsFromTo("<p class=\"code shift target\">","</p>");
                    if (!string.IsNullOrEmpty(redirect) && !string.IsNullOrEmpty(redirectUrl))
                    {
                        lock (locker)
                        {
                            redirects.Add(new Redirects
                            {
                                RedirectCode = redirect,
                                RedirectLink = redirectUrl
                            });
                        }
                    }

                    if (linkParser.Data.Contains("Too Many Requests") || linkParser.IsError)
                    {
                        Thread.Sleep(10000);
                    }
                } while (linkParser.Data.Contains("Too Many Requests")|| linkParser.IsError);
            });
            _site.Redirects = redirects;
        }
        
    }
}
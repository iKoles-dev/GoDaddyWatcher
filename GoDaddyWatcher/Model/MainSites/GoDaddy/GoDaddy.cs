using System.Linq;
using GoDaddyWatcher.Database;
using Homebrew.Additional;
using Homebrew.Enums;
using Homebrew.ParserComponents;

namespace GoDaddyWatcher.Model.MainSites.GoDaddy
{
    public class GoDaddy : Aggregator
    {
        private const int MaxPageNumber = 30;

        public override void CrawlData()
        {
            string baid = "-1";
            for (int i = 0; i < MaxPageNumber; i++)
            {
                LinkParser linkParser;
                do
                {
                    ReqParametres reqParametres = new ReqParametres("https://ru.auctions.godaddy.com/trpSearchResults.aspx",HttpMethod.Post,$"t=5&action=search&hidAdvSearch=ddlAdvKeyword:3|txtKeyword:|ddlCharacters:0|txtCharacters:|txtMinTraffic:|txtMaxTraffic:|txtMinDomainAge:|txtMaxDomainAge:|txtMinPrice:|txtMaxPrice:|ddlCategories:0|chkAddBuyNow:false|chkAddFeatured:false|chkAddDash:true|chkAddDigit:true|chkAddWeb:false|chkAddAppr:false|chkAddInv:false|chkAddReseller:false|ddlPattern1:|ddlPattern2:|ddlPattern3:|ddlPattern4:|chkSaleOffer:false|chkSalePublic:false|chkSaleExpired:false|chkSaleCloseouts:false|chkSaleUsed:false|chkSaleBuyNow:false|chkSaleDC:false|chkAddOnSale:false|ddlAdvBids:0|txtBids:|txtAuctionID:|ddlDateOffset:&rtr=7&baid={baid}&searchDir=1");
                    reqParametres.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.38 Safari/537.36");
                    reqParametres.RowRequest.ContentType = "application/x-www-form-urlencoded";
                    linkParser = new LinkParser(reqParametres.Request);
                } while (!linkParser.Data.Contains("onclick=\"_s_baid"));
                
                Sites.AddRange(linkParser.Data.ParsRegex("alt=\"\" />(.*?)<",1).Select(x=>new Site{Link = x}));
                
                baid = linkParser.Data.ParsFromTo("onclick=\"_s_baid1=", ";");
                if (string.IsNullOrEmpty(baid))
                {
                    break;
                }
            }
        }
    }
}
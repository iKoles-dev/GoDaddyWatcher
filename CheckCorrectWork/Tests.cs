using System;
using GoDaddyWatcher.Database;
using GoDaddyWatcher.Model;
using NUnit.Framework;

namespace CheckCorrectWork
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void ValidTest1()
        {
            Site site = new Site()
            {
                Link = "cyborgmanifesto.org"
            };
            SiteChecker siteChecker = new SiteChecker(site);
            siteChecker.Check();
            // if (site.Bl != 1 || site.TrustFlow!=0 || site.CitationFlow!=5)
            // {
            //     Assert.Fail();
            // }
            Assert.True(siteChecker.FitsRequirements);
        }
        [Test]
        public void ValidTest2()
        {
            Site site = new Site
            {
                Link = "infofemmes-pch.org"
            };
            SiteChecker siteChecker = new SiteChecker(site);
            siteChecker.Check();
            Assert.True(!siteChecker.FitsRequirements);
        }
    }
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Documents;
using GoDaddyWatcher.Database;
using GoDaddyWatcher.Model;
using GoDaddyWatcher.View;

namespace GoDaddyWatcher
{
    public static class ControlsContainer
    {
        public static int Bl, TrustFlow, CitationFlow;
        public static TextBlock Stats;
        public static int FoundSites;
        public static long FoundFittedSites, EndWebarchive, EndCheckTrust, StartWhois, AllCheckTrust;
        public static ObservableCollection<SiteView> GoodUsers = new ObservableCollection<SiteView>();
        public static List<SiteView> ManualUsers = new List<SiteView>();
    }
}
using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace GoDaddyWatcher.HomeBrew
{
    public static class Proxies
    {
        private static List<string> _proxies = new List<string>();
        private static readonly object Locker = new object();
        public static string Login = "";
        public static string Password = "";
        public static void AddProxies(List<string> proxies)
        {
            lock (Locker)
            {
                _proxies = proxies;
            }
        }

        public static string GetProxy()
        {
            lock (Locker)
            {
                return _proxies[new Random().Next(0, _proxies.Count - 1)];
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using GoDaddyWatcher.Model;
using Microsoft.EntityFrameworkCore;

namespace GoDaddyWatcher.View
{
    public partial class ManualLoad : Window
    {
        private static long _threadCount;
        public ManualLoad()
        {
            InitializeComponent();
        }

        private void ProcessData_Click(object sender, RoutedEventArgs e)
        {
            ProcessData.IsEnabled = false;
            new Thread(StartParsing){IsBackground = true}.Start();
        }

        private void StartParsing()
        {
            ControlsContainer.ManualUsers = new List<SiteView>();
            string[] sites = { };
            Application.Current.Dispatcher.Invoke(() =>
            {
                sites = new TextRange(Text.Document.ContentStart, Text.Document.ContentEnd).Text.Split(Convert.ToChar("\n"));
            });
            sites.Select(x=>x.Replace("\r","")).Where(x=>!string.IsNullOrEmpty(x)).Select(x=>new Database.Site{Link = x}).ToList().ForEach(x =>
            {
                Interlocked.Increment(ref _threadCount);
                var temp = x;
                new Thread(() =>
                {
                    SiteChecker siteChecker = new SiteChecker(temp);
                    siteChecker.Check();
                    if (siteChecker.FitsRequirements)
                    {
                        ControlsContainer.ManualUsers.Add(new SiteView(temp));
                    }

                    Interlocked.Decrement(ref _threadCount);
                }){IsBackground = true}.Start();
            });
            while (Interlocked.Read(ref _threadCount)>0)
            {
                Thread.Sleep(1000);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Text.Document.Blocks.Clear();;
                ProcessData.IsEnabled = true;
                ManualResult manualResult = new ManualResult();
                manualResult.Show();
            });
        }
    }
}
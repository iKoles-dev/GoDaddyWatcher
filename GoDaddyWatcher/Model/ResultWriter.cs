using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using GoDaddyWatcher.Database;

namespace GoDaddyWatcher.Model
{
    public static class ResultWriter
    {
        private static string _fileName;
        private static object _locker = new object();
        private static Stack<Site> _sites = new Stack<Site>();
        public static void Initialize()
        {
            if (File.Exists("result.txt"))
            {
                int iterator = 1;
                while (File.Exists($"result{iterator}.txt"))
                {
                    iterator++;
                }

                _fileName = $"result{iterator}.txt";
            }
            else
            {
                _fileName = "result.txt";
            }
            StartWriting();
        }

        public static void Add(Site site)
        {
            lock (_locker)
            {
                _sites.Push(site);
            }
        }

        private static void StartWriting()
        {
            using (StreamWriter streamWriter = new StreamWriter(_fileName))
            {
                while (true)
                {
                    lock (_locker)
                    {
                        if (_sites.Any())
                        {
                            var site = _sites.Pop();
                            streamWriter.WriteLine(site.ToString());
                            streamWriter.Flush();
                        }
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
        }
        
    }
}
﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
 using GoDaddyWatcher.Database;
 using GoDaddyWatcher.HomeBrew;
 using GoDaddyWatcher.Model;
 using GoDaddyWatcher.View;
 using Homebrew.Additional;
using Microsoft.EntityFrameworkCore;

namespace GoDaddyWatcher
{
    using System.IO;
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static List<string> _proxies;
        private static string _proxyLogin, _proxyPassword;
        private static int _bl = 0, _trustFlow = 0, _citationFlow = 0;

        private void OpenTable_Click(object sender, RoutedEventArgs e)
        {
            StandartTable table = new StandartTable();
            table.Show();
        }

        private void OpenDB_Click(object sender, RoutedEventArgs e)
        {
            DataBaseTable data = new DataBaseTable();
            data.Show();
        }

        private void ManualCheck_Click(object sender, RoutedEventArgs e)
        {
            ManualLoad manual = new ManualLoad();
            manual.Show();
        }

        private void BlTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!Regex.IsMatch(BlTextBox.Text, "^[0-9]+$"))
            {
                BlTextBox.Text = "0";
            }
            else
            {
                _bl = int.Parse(BlTextBox.Text);
            }

            ControlsContainer.Bl = _bl;
        }

        private void TrustFlowTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!Regex.IsMatch(TrustFlowTextBox.Text, "^[0-9]+$"))
            {
                TrustFlowTextBox.Text = "0";
            }
            else
            {
                _trustFlow = int.Parse(TrustFlowTextBox.Text);
            }

            ControlsContainer.TrustFlow = _trustFlow;
        }

        private void CitationFlow_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!Regex.IsMatch(CitationFlow.Text, "^[0-9]+$"))
            {
                CitationFlow.Text = "0";
            }
            else
            {
                _citationFlow = int.Parse(CitationFlow.Text);
            }

            ControlsContainer.CitationFlow = _citationFlow;
        }

        public MainWindow()
        {
            InitializeComponent();
            using (var db = new MyDbContext())
            {
                db.Database.Migrate();
            }
            using (var db = new MyDbContext())
            {
                ControlsContainer.Stats = StatsBlock;
                ControlsContainer.Stats.Text =
                    $"Всего сайтов в базе: {db.Sites.Count()}\nНайдено за сеанс: {ControlsContainer.FoundSites}\nНайдено подходящих за сеанс: {ControlsContainer.FoundFittedSites}";
            }
        }

        private void LoadProxy_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                InitialDirectory = Directory.GetCurrentDirectory()
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var fileText = File.ReadAllText(filePath);
                var proxies = fileText.ParsRegex("[0-9.]{9,}:[0-9]{3,}");
                if (proxies.Count == 0)
                {
                    MessageBox.Show("В указанном файле не обнаружены прокси.\nПопробуйте загрузить другой файл.");
                    return;
                }

                Proxies.Login = Login.Text;
                Proxies.Password = Password.Text;
                Proxies.AddProxies(proxies);
                MessageBox.Show("Прокси-файл успешно загружен!");
                LoadProxy.IsEnabled = false;
                StartWatching.IsEnabled = true;
                ManualCheck.IsEnabled = true;
            }
        }

        private void StartWatching_Click(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(BlTextBox.Text, "^[0-9]+$"))
            {
                BlTextBox.Text = "0";
            }
            else
            {
                _bl = int.Parse(BlTextBox.Text);
            }

            ControlsContainer.Bl = _bl;
            if (!Regex.IsMatch(TrustFlowTextBox.Text, "^[0-9]+$"))
            {
                TrustFlowTextBox.Text = "0";
            }
            else
            {
                _trustFlow = int.Parse(TrustFlowTextBox.Text);
            }

            ControlsContainer.TrustFlow = _trustFlow;
            if (!Regex.IsMatch(CitationFlow.Text, "^[0-9]+$"))
            {
                CitationFlow.Text = "0";
            }
            else
            {
                _citationFlow = int.Parse(CitationFlow.Text);
            }

            ControlsContainer.CitationFlow = _citationFlow;
            StartWatching.IsEnabled = false;
            new Thread(() =>
            {
                new Thread(ResultWriter.Initialize){IsBackground = true}.Start();
                new SiteWatcher();
            }){IsBackground = true}.Start();
        }
    }
}
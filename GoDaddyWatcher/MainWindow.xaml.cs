using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using Homebrew.Additional;

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
        public MainWindow()
        {
            InitializeComponent();
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

                _proxies = proxies;
                var login = fileText.ParsFromTo("log ", " ");
                var password = fileText.ParsFromTo("pas  ", " ");
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("В указанном файле не обнаружены логин/пароль к прокси.\nПопробуйте загрузить другой файл.");
                    return;
                }

                _proxyLogin = login;
                _proxyPassword = password;
                MessageBox.Show("Прокси-файл успешно загружен!");
                LoadProxy.IsEnabled = false;
                StartWatching.IsEnabled = true;
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
            if (!Regex.IsMatch(TrustFlowTextBox.Text, "^[0-9]+$"))
            {
                TrustFlowTextBox.Text = "0";
            }
            else
            {
                _bl = int.Parse(TrustFlowTextBox.Text);
            }
            if (!Regex.IsMatch(CitationFlow.Text, "^[0-9]+$"))
            {
                CitationFlow.Text = "0";
            }
            else
            {
                _bl = int.Parse(CitationFlow.Text);
            }
            new Thread(() =>
            {
                
            }){IsBackground = true}.Start();
        }
    }
}
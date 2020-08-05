using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GoDaddyWatcher.Database;
using GoDaddyWatcher.Model;

namespace GoDaddyWatcher.View
{
    public partial class ManualResult : Window
    {
        public ManualResult()
        {
            InitializeComponent();
            DataGrid.ItemsSource = ControlsContainer.ManualUsers;
        }
    }
}
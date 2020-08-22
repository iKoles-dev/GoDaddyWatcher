using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GoDaddyWatcher.View
{
    public partial class StandartTable : Window
    {
        public StandartTable()
        {
            InitializeComponent();
            DataGrid.ItemsSource = ControlsContainer.GoodUsers;
            ControlsContainer.GoodUsers.CollectionChanged += UpdateCollection;
        }
        private void UpdateCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
            DataGrid.ItemsSource = ControlsContainer.GoodUsers;
        }
    }
}

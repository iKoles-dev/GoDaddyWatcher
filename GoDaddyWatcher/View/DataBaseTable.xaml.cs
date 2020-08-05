using System.Linq;
using System.Threading;
using System.Windows;
using GoDaddyWatcher.Database;
using GoDaddyWatcher.Model;

namespace GoDaddyWatcher.View
{
    public partial class DataBaseTable : Window
    {
        public DataBaseTable()
        {
            InitializeComponent();
            using (var db = new MyDbContext())
            {
                var toSkip = db.Sites.Count() - 1000;
                if (toSkip < 0)
                {
                    toSkip = 0;
                }
                var data = db.Sites.Skip(toSkip).Select(x=> new SiteView(x)).ToList();
                DataGrid.ItemsSource = data;
            }
        }
    }
}
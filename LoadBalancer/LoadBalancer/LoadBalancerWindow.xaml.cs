using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LoadBalancer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LoadBalancerImpl lb;
        private ObservableCollection<Server> ServerList = new ObservableCollection<Server>();

        public MainWindow()
        {
            InitializeComponent();
            //ServerListDataGrid.DataContext = ServerList;
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            lb = new LoadBalancerImpl(port: 8085);
            lb.Listen();

            //TODO: Add eventhandler to add server to update ServerList.
            lb.AddServer("127.0.0.1", 8081);
        }
    }
}

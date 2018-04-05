using LBAlgorithm;
using ServerAffinity;
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
            Algorithms.ItemsSource = Utils.GetAssemblyNamesForTypes(typeof(ILBAlgorithm));
            ServerAffinities.ItemsSource = Utils.GetServerAffinitys(typeof(IServerAffinity<,>));
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            lb = new LoadBalancerImpl(port: 8080);
            lb.Listen();

            //TODO: Add eventhandler to add server to update ServerList.
            //lb.AddServer("127.0.0.1", 8081);
        }

        private void AddServer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (int.TryParse(ServerPortTxt.Text, out int port))
                {
                    if (port <= 0)
                    {
                        Console.WriteLine("Please enter a positive port number.");
                    }
                    else
                    {
                        if (lb == null) Console.WriteLine("Can't add servers before starting the load balancer.");
                        else
                        {
                            lb.AddServer("127.0.0.1", port);
                            Console.WriteLine($"Added server with port: {port}");
                        }
                    }
                }
            }
        }
    }
}

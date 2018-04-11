using LBAlgorithm;
using Messages;
using ServerAffinity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
        private Dictionary<string, string> algorithms = Utils.GetAssemblyNamesForTypes(typeof(ILBAlgorithm));
        private Dictionary<string, string> affinities = Utils.GetAssemblyNamesForTypes(typeof(IServerAffinity));

        public MainWindow()
        {
            InitializeComponent();
            ServerListDataGrid.ItemsSource = ServerList;
            Algorithms.ItemsSource = algorithms.Keys;
            ServerAffinities.ItemsSource = affinities.Keys;
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            lb = new LoadBalancerImpl(port: 8080);
            lb.Listen();
            Task.Run(() => lb.CalculateServersLatency(5000, RefreshServersDataGrid));
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
                            lb.AddServer("127.0.0.1", port, (server) => ServerList.Add(server));
                            Console.WriteLine($"Added server with port: {port}");
                        }
                    }
                }
            }
        }

        private void Algorithms_SelectedIndexChanged(object sender, EventArgs e)
        {
            string algorithmName = Algorithms.SelectedItem.ToString();

            string algorithmDll = algorithms[algorithmName];

            Assembly assembly = Assembly.LoadFrom(algorithmDll);

            Type type = assembly.GetType(algorithmName);

            Console.WriteLine(type.ToString());

            //Assembly.

            //Type type = assembly.GetType(algorithmName);

            //foreach(Type type in types)
            //{
            //    Console.WriteLine(type.ToString());
            //}

            dynamic algorithm = Activator.CreateInstance(type);

            if(lb != null)
            {
                lb.Algorithm = algorithm;
            }
        }

        private void ServerAffinity_SelectedIndexChanged(object sender, EventArgs e)
        {
            string affinityName = ServerAffinities.SelectedItem.ToString();

            string affinityDLL = affinities[affinityName];

            Assembly assembly = Assembly.LoadFrom(affinityDLL);

            Type type = assembly.GetType(affinityName);

            Console.WriteLine(type.ToString());

            dynamic affinity = Activator.CreateInstance(type);

            if (lb != null)
            {
                lb.Sessions = affinity;
            }
        }

        private void RefreshServersDataGrid()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => RefreshServersDataGrid());
                return;
            }

            Console.WriteLine("Refreshing Datagrid.");
            ServerListDataGrid.Items.Refresh();

        }
    }
}

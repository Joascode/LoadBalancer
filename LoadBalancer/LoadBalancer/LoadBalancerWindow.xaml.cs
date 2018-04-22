using LBAlgorithm;
using Messages;
using ServerAffinity;
using ServerChatters;
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
        private ObservableCollection<ServerChatter> ServerList = new ObservableCollection<ServerChatter>();
        private Dictionary<string, string> algorithms = Utils.GetAssemblyNamesForType(typeof(ILBAlgorithm));
        private Dictionary<string, string> affinities = Utils.GetAssemblyNamesForType(typeof(IServerAffinity));
        private bool started = false;
        private int port = 8080;

        public MainWindow()
        {
            InitializeComponent();
            ServerListDataGrid.ItemsSource = ServerList;
            Algorithms.ItemsSource = algorithms.Keys;
            ServerAffinities.ItemsSource = affinities.Keys;
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if(started)
            {
                started = false;
                StartBtn.Content = "Start";
                lb.Stop();
            }
            else
            {
                lb = new LoadBalancerImpl(port: port);
                lb.Listen();
                Task.Run(() => lb.CalculateServersLatency(2000, RefreshServersDataGrid));
                started = true;
                StartBtn.Content = "Stop";
            }
        }

        private void SetLBPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (int.TryParse(ServerPortTxt.Text, out int newPort))
                {
                    if (newPort <= 0)
                    {
                        Console.WriteLine("Please enter a positive port number.");
                    }
                    else
                    {
                        if (lb != null) Console.WriteLine("Can't adjust port when load balancer is listening.");
                        else
                        {
                            port = newPort;
                        }
                    }
                }
            }
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
                        }
                    }
                }
            }
        }

        private void RemoveServer_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                if (int.TryParse(ServerRemoveTxt.Text, out int serverId))
                {
                    lb.RemoveServer(serverId.ToString(), (server) => ServerList.Remove(server));
                    
                }
            }
        }

        private void ReconnectServer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (int.TryParse(ServerReconnectTxt.Text, out int serverId))
                {
                    lb.ReconnectServer(serverId.ToString());
                }
            }
        }

        private void Algorithms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Algorithms.SelectedItem != null)
            {
                string algorithmName = Algorithms.SelectedItem.ToString();

                string algorithmDll = algorithms[algorithmName];

                Assembly assembly = Assembly.LoadFrom(algorithmDll);

                Type type = assembly.GetType(algorithmName);

                Console.WriteLine(type.ToString());

                dynamic algorithm = Activator.CreateInstance(type);

                if (lb != null)
                {
                    lb.Algorithm = algorithm;
                }
            }
        }

        private void ServerAffinity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(ServerAffinities.SelectedItem != null)
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
        }

        private void ClearAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            Algorithms.UnselectAll();
            if (lb != null)
            {
                lb.Algorithm = null;
                Console.WriteLine("Clearing set algorithms.");
            }
        }

        private void ReloadAlgorithms_Click(object sender, RoutedEventArgs e)
        {
            algorithms = Utils.GetAssemblyNamesForType(typeof(ILBAlgorithm));
            Algorithms.ItemsSource = algorithms.Keys;
            Console.WriteLine("Reload algorithm assemblies.");
        }

        private void ClearAffinity_Click(object sender, RoutedEventArgs e)
        {
            ServerAffinities.UnselectAll();
            if (lb != null)
            {
                lb.Sessions = null;
                Console.WriteLine("Clearing set affinities.");
            }
        }

        private void ReloadAffinity_Click(object sender, RoutedEventArgs e)
        {
            affinities = Utils.GetAssemblyNamesForType(typeof(IServerAffinity));
            ServerAffinities.ItemsSource = affinities.Keys;
            Console.WriteLine("Reload affinity assemblies.");
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

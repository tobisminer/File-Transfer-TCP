using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace FileTransferServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateServerBtn_Click(object sender, RoutedEventArgs e)
        { 
            if (int.TryParse(PortBox.Text, out var port) == false)
            {
                MessageBox.Show("Port must be numeric!", "Port error", MessageBoxButton.OK);
                return;
            }
            var window = new ServerWindow(port);
            window.Show();

        }

        private void CreateClient_Click(object sender, RoutedEventArgs e)
        {
            var client = new Client();
            client.Show();
        }
    }
}

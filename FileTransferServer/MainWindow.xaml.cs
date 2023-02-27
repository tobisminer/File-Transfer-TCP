using System.Windows;

namespace FileTransferServer;

/// <summary>
///     Interaction logic for MainWindow.xaml
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
        Hide();
        window.ShowDialog();
        Show();
        window.Close();
    }

    private void CreateClient_Click(object sender, RoutedEventArgs e)
    {
        var client = new Client();
        Hide();
        client.ShowDialog();
        Show();
    }
}
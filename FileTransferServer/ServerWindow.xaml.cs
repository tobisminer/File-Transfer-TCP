using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Win32;


namespace FileTransferServer
{
    /// <summary>
    /// Interakční logika pro ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        public int Port;
        public TcpListener Listener;
        public ServerWindow(int port)
        {
            InitializeComponent();
            Port = port;
            CreateServer();
            IpLabel.Content = "Server IP: " + GetLocalIpAddress();
            PortLabel.Content = "Server Port: " + Port;
        }
        public static IPAddress GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
       private async void CreateServer()
        {
            if (Port is < 0 or > 65535)
            {
                MessageBox.Show("Port must be number between 0-65535!", "Port error", MessageBoxButton.OK);
            }
            Listener = new TcpListener(GetLocalIpAddress(), Port);
            Listener.Start();
            var socket = await Listener.AcceptSocketAsync();
            
            Output.Text += $"Client connected! With IP {socket.RemoteEndPoint}" + Environment.NewLine;
            
            const int bufferSize = 1024;
            
            var header = new byte[bufferSize];
            socket.Receive(header);
            var headerStr = Encoding.ASCII.GetString(header);
            var split = headerStr.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var headers = split.Where(
                s => s.Contains(':')).ToDictionary(
                s => s[..s.IndexOf(":", StringComparison.Ordinal)],
                s => s[(s.IndexOf(":", StringComparison.Ordinal) + 1)..]);
            
            var fileSize = Convert.ToInt32(headers["Content-length"]);
            var bufferCount = Convert.ToInt32(Math.Ceiling((double)fileSize / (double)bufferSize));
            var filename = headers["Filename"];
            var dialog = new SaveFileDialog
            {
                FileName = filename
            };
            dialog.ShowDialog();
            var fs = new FileStream(dialog.FileName, FileMode.OpenOrCreate);
            
            var count = 0;
            while (fileSize > 0)
            {
                count++;
                var buffer = new byte[bufferSize];
                var size = await socket.ReceiveAsync(buffer, SocketFlags.Partial);
                fs.Write(buffer, 0, size);
                fileSize -= size;
                //update progress bar
                FileBar.Value = count * 100 / (double)bufferCount;
            }
            fs.Close();
            socket.Close();
            Listener.Stop();
            Output.Text += "File transfer done!" + Environment.NewLine;
            CreateServer();
        }
    }
}

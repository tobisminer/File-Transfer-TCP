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
            var listener = new TcpListener(GetLocalIpAddress(), Port);
            listener.Start();
            var socket = await listener.AcceptSocketAsync();
            //get ip and port of client
            var ip = socket.RemoteEndPoint?.ToString()?.Split(':')[0];
            var port = socket.RemoteEndPoint?.ToString()?.Split(':')[1];
            Output.Text += $"Client connected! With IP {ip}:{port}" + Environment.NewLine;
            
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
            
            var filename = headers["Filename"];
            var dialog = new SaveFileDialog
            {
                FileName = filename
            };
            dialog.ShowDialog();
            var fs = new FileStream(dialog.FileName, FileMode.OpenOrCreate);
            while (fileSize > 0)
            {
                var buffer = new byte[bufferSize];
                var size = await socket.ReceiveAsync(buffer, SocketFlags.Partial);
                fs.Write(buffer, 0, size);
                fileSize -= size;
            }
            fs.Close();
            socket.Close();
            listener.Stop();
        }
    }
}

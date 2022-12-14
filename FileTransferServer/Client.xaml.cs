using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace FileTransferServer
{
    /// <summary>
    /// Interakční logika pro Client.xaml
    /// </summary>
    public partial class Client : Window
    {
        public Client()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var file = new OpenFileDialog();
            file.ShowDialog();

            const int bufferSize = 1024;

            var fs = new FileStream(file.FileName, FileMode.Open);
            var bufferCount = Convert.ToInt32(Math.Ceiling(fs.Length / (double)bufferSize));

            var tcpClient = new TcpClient("192.168.1.57", 23000)
            {
                SendTimeout = 600000,
                ReceiveTimeout = 600000
            };

            var headerStr = "Content-length:" + fs.Length + "\r\nFilename:" + file.SafeFileName + "\r\n";
            var header = new byte[bufferSize];
            Array.Copy(Encoding.ASCII.GetBytes(headerStr), header, Encoding.ASCII.GetBytes(headerStr).Length);

            tcpClient.Client.Send(header);
            for (var i = 0; i < bufferCount; i++)
            {
                var buffer = new byte[bufferSize];
                var size = fs.Read(buffer, 0, bufferSize);
                tcpClient.Client.Send(buffer, size, SocketFlags.Partial);
            }
            tcpClient.Client.Close();
            fs.Close();
        }
    }
}

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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.ShowDialog();
            if (fileDialog.FileName == "")
            {
                return;
            }

            for (var fileCount = 0; fileCount < fileDialog.FileNames.Length; fileCount++)
            {
                var fileName = fileDialog.FileNames[fileCount];
                var safeFileName = fileDialog.SafeFileNames[fileCount];
                const int bufferSize = 1024;

                var fs = new FileStream(fileName, FileMode.Open);
                var bufferCount = Convert.ToInt32(Math.Ceiling(fs.Length / (double)bufferSize));

                var tcpClient = new TcpClient(IPaddressBox.Text, int.Parse(PortBox.Text))
                {
                    SendTimeout = 600000,
                    ReceiveTimeout = 600000
                };
                var client = tcpClient.Client;
                var headerStr = "Content-length:" + fs.Length + "\r\nFilename:" + safeFileName + "\r\n";
                var header = new byte[bufferSize];
                Array.Copy(Encoding.ASCII.GetBytes(headerStr), header, Encoding.ASCII.GetBytes(headerStr).Length);

                await client.SendAsync(header);
                for (var i = 0; i < bufferCount; i++)
                {
                    var buffer = new byte[bufferSize];
                    var size = await fs.ReadAsync(buffer.AsMemory(0, bufferSize));

                    client.Send(buffer, size, SocketFlags.Partial);
                }

                client.Close();
                fs.Close();
            }
        }
    }
}

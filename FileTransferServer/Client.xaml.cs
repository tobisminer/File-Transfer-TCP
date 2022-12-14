using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
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
            //var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.1.57"), 23000));
            //
            //await client.SendAsync(fileNameBytes, SocketFlags.None);
            //TcpClient client = new TcpClient();
            //client.Connect("localhost", 1234);

            //string message = "Hello, World!";
            //byte[] data = Encoding.UTF8.GetBytes(message);
            //client.GetStream().Write(data, 0, data.Length);

            //long number = 1234567890;
            //byte[] numberData = BitConverter.GetBytes(number);
            //client.GetStream().Write(numberData, 0, numberData.Length);


            //var fileBytes = await System.IO.File.ReadAllBytesAsync(file.FileName);
            //_ = await client.SendAsync(fileBytes, SocketFlags.None);
            //MessageBox.Show("File sent!");
            //var message = InputBox.Text + "<|EOM|>";
            //var messageBytes = Encoding.UTF8.GetBytes(message);
            //var _ = await client.SendAsync(messageBytes, SocketFlags.None);
            //client.Shutdown(SocketShutdown.Both);
            //var file = new OpenFileDialog();
            //file.ShowDialog();
            //using var client = new TcpClient("192.168.1.57", 23000);
            //await using (var stream = client.GetStream())
            //{
            //    var fileNameBytes = Encoding.UTF8.GetBytes(file.SafeFileName);
            //    stream.Write(fileNameBytes, 0, fileNameBytes.Length);
            //    await using (var fileStream = File.OpenRead(file.FileName))
            //    {
            //        var buffer = new byte[1024];
            //        int bytesRead;
            //        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            //        {
            //            stream.Write(buffer, 0, bytesRead);
            //        }
            //    }
            //}
            var file = new OpenFileDialog();
            file.ShowDialog();

            int bufferSize = 1024;
            byte[] buffer = null;
            byte[] header = null;


            FileStream fs = new FileStream(file.FileName, FileMode.Open);
            bool read = true;

            int bufferCount = Convert.ToInt32(Math.Ceiling((double)fs.Length / (double)bufferSize));



            var tcpClient = new TcpClient("192.168.1.57", 23000);
            tcpClient.SendTimeout = 600000;
            tcpClient.ReceiveTimeout = 600000;

            string headerStr = "Content-length:" + fs.Length + "\r\nFilename:" + file.SafeFileName + "\r\n";
            header = new byte[bufferSize];
            Array.Copy(Encoding.ASCII.GetBytes(headerStr), header, Encoding.ASCII.GetBytes(headerStr).Length);

            tcpClient.Client.Send(header);

            for (int i = 0; i < bufferCount; i++)
            {
                buffer = new byte[bufferSize];
                int size = fs.Read(buffer, 0, bufferSize);

                tcpClient.Client.Send(buffer, size, SocketFlags.Partial);

            }

            tcpClient.Client.Close();

            fs.Close();
        }
    }
}

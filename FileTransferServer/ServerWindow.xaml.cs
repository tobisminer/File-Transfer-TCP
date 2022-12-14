using System;
using System.Collections.Generic;
using System.IO;
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
        public int port;
        private Socket server;
        public ServerWindow(int port)
        {
            InitializeComponent();
            this.port = port;
            new Thread(CreateServer).Start();

        }
        public static IPAddress GetLocalIPAddress()
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
            if (port is < 0 or > 65535)
            {
                MessageBox.Show("Port musí být číslo v rozmezí 0-65535!", "Port error", MessageBoxButton.OK);
            }
            var listener = new TcpListener(GetLocalIPAddress(), port);
            listener.Start();
            Socket socket = listener.AcceptSocket();
            int bufferSize = 1024;
            byte[] buffer = null;
            byte[] header = null;
            string headerStr = "";
            string filename = "";
            int filesize = 0;


            header = new byte[bufferSize];

            socket.Receive(header);

            headerStr = Encoding.ASCII.GetString(header);


            string[] splitted = headerStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (string s in splitted)
            {
                if (s.Contains(":"))
                {
                    headers.Add(s.Substring(0, s.IndexOf(":")), s.Substring(s.IndexOf(":") + 1));
                }

            }
            //Get filesize from header
            filesize = Convert.ToInt32(headers["Content-length"]);
            //Get filename from header
            filename = headers["Filename"];
            var dialog = new SaveFileDialog
            {
                FileName = filename
            };
            dialog.ShowDialog();
            var bufferCount = Convert.ToInt32(Math.Ceiling((double)filesize / (double)bufferSize));
            var fs = new FileStream(dialog.FileName, FileMode.OpenOrCreate);
            while (filesize > 0)
            {
                buffer = new byte[bufferSize];
                var size = socket.Receive(buffer, SocketFlags.Partial);
                fs.Write(buffer, 0, size);
                filesize -= size;
            }
            fs.Close();

        }

        private void CheckConnection(TcpClient clientObject)
        {
            var client = clientObject;
            //var stream = client.GetStream();
            //var reader = new BinaryReader(stream);
            //var fileName = reader.ReadString();
            //var fileData = reader.ReadBytes((int)reader.BaseStream.Length);
            //var dialog = new SaveFileDialog
            //{
            //    FileName = fileName
            //};

            //dialog.ShowDialog();
            //var path = dialog.FileName;
            //var file = new FileStream(path, FileMode.Create);
            //file.Write(fileData, 0, fileData.Length);
            //file.Close();
            //client.Close();
            
            var stream = client.GetStream();
            var reader = new BinaryReader(stream);

            var fileName = reader.ReadString();

            var buffer = new byte[1024];
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                int bytesRead;
                do
                {
                    bytesRead = client.GetStream().Read(buffer, 0, buffer.Length);
                    fileStream.Write(buffer, 0, bytesRead);
                } while (bytesRead == buffer.Length);
            }

            // Uzavření spojení s klientem
           // client.Close();
        }
    }
}

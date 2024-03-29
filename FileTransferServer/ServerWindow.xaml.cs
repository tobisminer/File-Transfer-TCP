﻿using FileTransferServer.Properties;
using Ookii.Dialogs.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace FileTransferServer;

/// <summary>
///     Interakční logika pro ServerWindow.xaml
/// </summary>
public partial class ServerWindow : Window
{
    public TcpListener Listener;
    public int Port;

    public ServerWindow(int port)
    {
        InitializeComponent();
        PathLabel.Content = Settings.Default.OutputFolder;
        Port = port;
        Listener = new TcpListener(GetLocalIpAddress(), Port);
        Listener.Start();
        CreateServer();
        IpLabel.Content = "Server IP: " + GetLocalIpAddress();
        PortLabel.Content = "Server Port: " + Port;
    }

    public static IPAddress GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip;
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
    private async void CreateServer()
    {
        if (Port is < 0 or > 65535)
            MessageBox.Show("Port must be number between 0-65535!", "Port error", MessageBoxButton.OK);
        var watch = new Stopwatch();
        while (true)
        {
            try
            {
                var socket = await Listener.AcceptSocketAsync();
                watch.Restart();
                Output.Items.Add($"Client connected! With IP {socket.RemoteEndPoint}");

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
                var bufferCount = Convert.ToInt32(Math.Ceiling(fileSize / (double)bufferSize));
                var filename = headers["Filename"];
                Output.Items.Add($"File name: {filename}");
                Output.Items.Add($"File size: {fileSize} bytes");

                if (PathLabel.Content == "")
                    MessageBox.Show("Please select a path to save the file! Saving to default spot, near exe file.",
                        "Path error", MessageBoxButton.OK);
                Output.Items.Add($"Saving to: {PathLabel.Content + filename}");
                var fs = new FileStream(PathLabel.Content + filename, FileMode.OpenOrCreate);

                var count = 0;
                while (fileSize > 0)
                {
                    count++;
                    var buffer = new byte[bufferSize];
                    var size = await socket.ReceiveAsync(buffer, SocketFlags.Partial);
                    fs.Write(buffer, 0, size);
                    fileSize -= size;

                    FileBar.Value = count * 100 / (double)bufferCount;
                }

                fs.Close();
                socket.Close();
                watch.Stop();
                Output.Items.Add($"File transfered in {watch.ElapsedMilliseconds} ms");
                Output.Items.Add("---------File transfer done!---------");
            }
            catch (Exception e)
            {
                Listener.Stop();
            }
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Please select a folder.",
            UseDescriptionForTitle = true
        };
        var result = dialog.ShowDialog();
        if (result != true) return;
        var path = dialog.SelectedPath;
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        path += "\\";
        PathLabel.Content = path;
        Settings.Default.OutputFolder = path;
        Settings.Default.Save();
    }
}
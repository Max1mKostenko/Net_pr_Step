using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp11
{
    public static class Server
    {
        public static List<Client> clients = new();
        public static Socket? ServerSocket { get; set; }
        public static MainWindow? Window { get; set; }

        // -------------------------------------------
        public static void Start()
        {
            Window = Application.Current.MainWindow as MainWindow;
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
            ServerSocket.Listen(100);

            ListViewItem item = new ListViewItem() 
            {
                Foreground = new SolidColorBrush(Color.FromRgb(14, 130, 118)),
                Content = "Server ONLINE"
            };
            Window?.lvChat.Items.Add(item);

            Task.Run(WaitForClients);
        }

        // -------------------------------------------
        public static void WaitForClients() 
        {
            while (true)
            {
                Socket? clientSocket = ServerSocket?.Accept();
                Task.Run(() => ManageClient(clientSocket));
            }
        }

        // -------------------------------------------
        public static Client FillClient(Socket? clientSocket, string? login)
        {
            Random random = new Random();
            Client client = new Client() 
            {
                Login = login,
                Socket = clientSocket,
                DateOfConnection = DateTime.Now,
                ColorBrush = new SolidColorBrush(Color.FromRgb(
                    (byte)random.Next(0, 256),
                    (byte)random.Next(0, 256),
                    (byte)random.Next(0, 256)
                    ))
            };
            return client;
        }

        // -------------------------------------------
        public static void PrintMessage(string? message, Color color)
        {
            Window?.Dispatcher.Invoke(() => { 
                ListViewItem item = new ListViewItem() { Content = message, Foreground = new SolidColorBrush(color) };
                Window?.lvChat?.Items.Add(item);
                Window?.lvChat.ScrollIntoView(Window?.lvChat.Items[^1]);
            });
        }

        // -------------------------------------------
        public static void ManageClient(Socket? clientSocket)
        {
            bool clientFilled = false;
            Client client = new Client();

            byte[] buffer = new byte[1024];
            int bytesCount;

            try
            {
                while ((bytesCount = clientSocket.Receive(buffer)) > 0)
                {
                    if (!clientFilled)
                    {
                        string? login = Encoding.ASCII.GetString(buffer, 0, bytesCount);
                        client = FillClient(clientSocket, login);
                        clients.Add(client);
                        clientSocket.Send(Encoding.ASCII.GetBytes($"Welcome, {login}, to chat with {clients.Count} users!"));
                        clientFilled = true;

                        Window?.Dispatcher.Invoke(() => { Window?.lvClients.Items.Add(client); });
                        Window?.Dispatcher.Invoke(() => 
                        { 
                            Window?.lvChat.Items.Add($"{DateTime.Now} connected {login}");
                            Window?.lvChat.ScrollIntoView(Window?.lvChat.Items[^1]);
                        });
                    }
                    else
                    {
                        string? message = Encoding.ASCII.GetString(buffer, 0, bytesCount);
                        PrintMessage(message, client.ColorBrush.Color);
                    }
                }
            }
            catch (Exception ex) { Window?.Dispatcher.Invoke(() => { Window?.lvChat.Items.Add(ex.Message); }); }
            finally
            {
                clientSocket?.Close();
                clients.Remove(client);
                Window?.Dispatcher.Invoke(() => { Window?.lvClients.Items.Remove(client); });
                Window?.Dispatcher.Invoke(() => 
                {
                    Window?.lvChat.Items.Add($"{DateTime.Now} disconnected {client.Login}");
                    Window?.lvChat.ScrollIntoView(Window?.lvChat.Items[^1]);
                });
            }
        }
    }
}

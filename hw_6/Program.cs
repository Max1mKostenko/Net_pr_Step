using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConsoleApp47
{
    internal class Program
    {
        static class Server
        {
            static List<Socket> clients = new();
            static Socket server;
            static Timer timeTimer;
            static public void Start()
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80);
                server = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                    );
                try
                {
                    server.Bind(ep);
                    server.Listen(10);
                    Console.WriteLine("SERVER IS ON!");

                    timeTimer = new Timer(BroadcastTime, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

                    while (true)
                    {
                        Socket client = server.Accept();
                        clients.Add(client);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Connected: {client.RemoteEndPoint}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Task.Run(() => ManageClient(client));
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                finally
                {
                    timeTimer?.Dispose();
                    server.Shutdown(SocketShutdown.Both);
                    server.Close();
                }
            }
            static void BroadcastTime(object state)
            {
                string timeMessage = $"Current time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                byte[] timeData = Encoding.ASCII.GetBytes(timeMessage);

                List<Socket> clientsCopy;
                lock (clients)
                {
                    clientsCopy = new List<Socket>(clients);
                }

                foreach (Socket client in clientsCopy)
                {
                    try
                    {
                        client.Send(timeData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send time to client: {ex.Message}");
                        lock (clients)
                        {
                            clients.Remove(client);
                        }
                        try
                        {
                            client.Close();
                        }
                        catch { }
                    }
                }
            }
            static public void ManageClient(Socket client)
            {
                byte[] buffer = new byte[1024];
                int bytesCount;
                try
                {
                    while ((bytesCount = client.Receive(buffer)) > 0)
                    {
                        string message = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesCount);
                        Console.WriteLine($"Received from {clients.IndexOf(client)}: {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Client management error: {ex.Message}");
                }
                finally
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Disconnected: {client.RemoteEndPoint}");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    lock (clients)
                    {
                        clients.Remove(client);
                    }

                    try
                    {
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                    catch { }
                }
            }
        }
        static void Main(string[] args)
        {
            Server.Start();
        }
    }
}
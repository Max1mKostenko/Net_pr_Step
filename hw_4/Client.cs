using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MathGameClient
{
    static class Client
    {
        static Socket client;
        static string playerName = "";
        static int myScore = 0;

        static public void SetLogin()
        {
            Console.Write("Enter your name: ");
            Console.ForegroundColor = ConsoleColor.Green;
            string? login = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            playerName = string.IsNullOrEmpty(login) ? "Player" + new Random().Next(100, 999) : login;

            byte[] buffer = Encoding.UTF8.GetBytes(playerName);
            client.Send(buffer);
        }

        static public void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            int bytesCount;

            try
            {
                while ((bytesCount = client.Receive(buffer)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesCount);

                    if (message.StartsWith("QUESTION:"))
                    {
                        string question = message.Substring(9);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\n╔════════════════════════════╗");
                        Console.WriteLine($"║   NEW QUESTION: {question,-10} ║");
                        Console.WriteLine($"╚════════════════════════════╝");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else if (message.StartsWith("CORRECT:"))
                    {
                        string info = message.Substring(8);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n✓ {info}");

                        if (info.StartsWith(playerName))
                        {
                            myScore++;
                            Console.WriteLine($"🎉 Your score: {myScore}");
                        }

                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else if (message.StartsWith("WRONG:"))
                    {
                        string info = message.Substring(6);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"✗ {info}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else if (message.StartsWith("SYSTEM:"))
                    {
                        string info = message.Substring(7);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"[System] {info}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Connection lost: {e.Message}");
            }
        }

        static public void Start()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Console.WriteLine("=== MATH GAME CLIENT ===");
                Console.WriteLine("Connecting to server...");

                client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Connected to server!\n");
                Console.ForegroundColor = ConsoleColor.Gray;

                SetLogin();

                Task.Run(() => ReceiveMessages());

                Console.WriteLine("\nType your answers and press Enter to submit.");
                Console.WriteLine("Type 'quit' to exit.\n");

                while (true)
                {
                    Console.Write("Your answer: ");
                    string? answer = Console.ReadLine();

                    if (string.IsNullOrEmpty(answer))
                        continue;

                    if (answer.ToLower() == "quit")
                        break;

                    client.Send(Encoding.UTF8.GetBytes(answer));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("\nDisconnected from server. Press any key to exit...");
                Console.ReadKey();
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Client.Start();
        }
    }
}
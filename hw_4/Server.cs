using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MathGameServer
{
    internal class Program
    {
        static class Server
        {
            static List<Socket> clients = new();
            static List<string> logins = new();
            static Dictionary<string, int> scores = new();
            static Dictionary<string, int> attempts = new();
            static Socket server;
            static Random random = new Random();

            static int currentAnswer = 0;
            static string currentQuestion = "";
            static object lockObj = new object();

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
                    Console.WriteLine("=== MATH GAME SERVER IS ON ===");
                    Console.WriteLine("Waiting for players...\n");

                    GenerateQuestion();

                    while (true)
                    {
                        Socket client = server.Accept();
                        clients.Add(client);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Player connected: {client.RemoteEndPoint}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Task.Run(() => ManageClient(client));
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                finally
                {
                    server.Shutdown(SocketShutdown.Both);
                    server.Close();
                }
            }

            static void GenerateQuestion()
            {
                int num1 = random.Next(1, 21);
                int num2 = random.Next(1, 21);
                int operation = random.Next(0, 4);

                switch (operation)
                {
                    case 0:
                        currentQuestion = $"{num1} + {num2}";
                        currentAnswer = num1 + num2;
                        break;
                    case 1:
                        currentQuestion = $"{num1} - {num2}";
                        currentAnswer = num1 - num2;
                        break;
                    case 2:
                        currentQuestion = $"{num1} × {num2}";
                        currentAnswer = num1 * num2;
                        break;
                    case 3:
                        num1 = num1 * num2;
                        currentQuestion = $"{num1} ÷ {num2}";
                        currentAnswer = num1 / num2;
                        break;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[NEW QUESTION] {currentQuestion} = ?");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            static public void BroadcastMessage(string message)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                foreach (var client in clients)
                {
                    try
                    {
                        client.Send(buffer);
                    }
                    catch { }
                }
            }

            static void SendToClient(Socket client, string message)
            {
                try
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    client.Send(buffer);
                }
                catch { }
            }

            static void DisplayStatistics()
            {
                Console.WriteLine("\n=== CURRENT STATISTICS ===");
                var sortedScores = scores.OrderByDescending(x => x.Value);

                foreach (var player in sortedScores)
                {
                    int totalAttempts = attempts.ContainsKey(player.Key) ? attempts[player.Key] : 0;
                    double accuracy = totalAttempts > 0 ? (player.Value * 100.0 / totalAttempts) : 0;
                    Console.WriteLine($"{player.Key}: {player.Value} points | {totalAttempts} attempts | {accuracy:F1}% accuracy");
                }
                Console.WriteLine("========================\n");
            }

            static public void ManageClient(Socket client)
            {
                byte[] buffer = new byte[1024];
                int bytesCount;
                string playerLogin = "";

                try
                {
                    bytesCount = client.Receive(buffer);
                    playerLogin = Encoding.UTF8.GetString(buffer, 0, bytesCount).Trim();

                    lock (lockObj)
                    {
                        logins.Add(playerLogin);
                        scores[playerLogin] = 0;
                        attempts[playerLogin] = 0;
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Player '{playerLogin}' joined the game!");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    SendToClient(client, $"Welcome to Math Game, {playerLogin}!");
                    Thread.Sleep(100);
                    SendToClient(client, $"QUESTION:{currentQuestion}");

                    BroadcastMessage($"SYSTEM:{playerLogin} joined the game!");

                    while ((bytesCount = client.Receive(buffer)) > 0)
                    {
                        string answer = Encoding.UTF8.GetString(buffer, 0, bytesCount).Trim();

                        lock (lockObj)
                        {
                            attempts[playerLogin]++;

                            if (int.TryParse(answer, out int playerAnswer))
                            {
                                Console.WriteLine($"[{playerLogin}] answered: {playerAnswer}");

                                if (playerAnswer == currentAnswer)
                                {
                                    scores[playerLogin]++;

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"✓ {playerLogin} got it right! Answer: {currentAnswer}");
                                    Console.ForegroundColor = ConsoleColor.Gray;

                                    BroadcastMessage($"CORRECT:{playerLogin} answered correctly! The answer was {currentAnswer}");
                                    Thread.Sleep(500);

                                    GenerateQuestion();
                                    BroadcastMessage($"QUESTION:{currentQuestion}");

                                    DisplayStatistics();
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"✗ {playerLogin} answered incorrectly");
                                    Console.ForegroundColor = ConsoleColor.Gray;

                                    SendToClient(client, "WRONG:Incorrect! Try again.");
                                }
                            }
                            else
                            {
                                SendToClient(client, "WRONG:Please enter a valid number!");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error with {playerLogin}: {ex.Message}");
                }
                finally
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Player '{playerLogin}' disconnected");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    lock (lockObj)
                    {
                        int index = clients.IndexOf(client);
                        if (index >= 0)
                        {
                            clients.RemoveAt(index);
                            logins.RemoveAt(index);
                        }
                    }

                    BroadcastMessage($"SYSTEM:{playerLogin} left the game");

                    try { client.Close(); } catch { }
                }
            }
        }

        static void Main(string[] args)
        {
            Server.Start();
        }
    }
}
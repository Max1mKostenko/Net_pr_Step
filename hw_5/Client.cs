using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ConsoleApp66
{
    class Question
    {
        public string? Text { get; set; }
        public string? Correct { get; set; }
        public List<string>? Options { get; set; }
    }

    class Answer
    {
        public string? PlayerAnswer { get; set; }
    }

    class Result
    {
        public bool IsCorrect { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? Message { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient("127.0.0.1", 5050);
            NetworkStream stream = tcpClient.GetStream();
            Console.WriteLine("Connected to quiz server!\n");

            try
            {
                byte[] buffer = new byte[4096];

                while (true)
                {
                    int count = stream.Read(buffer, 0, buffer.Length);
                    if (count == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, count).Trim();
                    var q = JsonSerializer.Deserialize<Question>(message);

                    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    Console.WriteLine($"Question: {q.Text}");
                    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                    for (int i = 0; i < q.Options.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {q.Options[i]}");
                    }
                    Console.WriteLine();

                    Console.Write("Your answer: ");
                    string? userInput = Console.ReadLine();

                    string playerAnswer = userInput;
                    if (int.TryParse(userInput, out int optionNumber) &&
                        optionNumber >= 1 && optionNumber <= q.Options.Count)
                    {
                        playerAnswer = q.Options[optionNumber - 1];
                    }

                    var answer = new Answer { PlayerAnswer = playerAnswer };
                    string answerJson = JsonSerializer.Serialize(answer);
                    byte[] answerData = Encoding.UTF8.GetBytes(answerJson);
                    stream.Write(answerData, 0, answerData.Length);

                    count = stream.Read(buffer, 0, buffer.Length);
                    if (count == 0) break;

                    string resultMessage = Encoding.UTF8.GetString(buffer, 0, count).Trim();
                    var result = JsonSerializer.Deserialize<Result>(resultMessage);

                    Console.ForegroundColor = result.IsCorrect ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.WriteLine($"\n{result.Message}");
                    Console.ResetColor();

                    if (!result.IsCorrect)
                    {
                        Console.WriteLine($"Correct answer: {result.CorrectAnswer}");
                    }

                    Console.WriteLine("\nWaiting for next question...\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                stream.Close();
                tcpClient.Close();
            }
        }
    }
}
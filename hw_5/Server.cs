using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ConsoleApp65
{
    // ----------------------------
    class Question
    {
        public string? Text { get; set; }
        public string? Correct { get; set; }
        public List<string>? Options { get; set; }
    }

    // ----------------------------
    class Answer
    {
        public string? PlayerAnswer { get; set; }
    }

    // ----------------------------
    class Result
    {
        public bool IsCorrect { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? Message { get; set; }
    }

    // ----------------------------
    internal class Program
    {
        static HttpClient? http = new HttpClient();
        static TcpListener? tcpListener = null;
        static List<TcpClient> players = new List<TcpClient>();

        // ----------------------------
        static void Main(string[] args)
        {
            tcpListener = new TcpListener(IPAddress.Any, 5050);
            tcpListener.Start();
            Console.WriteLine("Server started on port 5050...");

            Task.Run(() =>
            {
                while (true)
                {
                    var client = tcpListener.AcceptTcpClient();
                    players.Add(client);

                    Task.Run(() => RunQuiz(client));

                    Console.WriteLine("Player connected, players count: " + players.Count);
                }
            });

            while (true) { }
        }

        // ----------------------------
        static async void RunQuiz(TcpClient player)
        {
            NetworkStream stream = player.GetStream();

            try
            {
                while (true)
                {
                    // Генеруємо і відправляємо питання
                    Question q = await GetQuestion();
                    string questionJson = JsonSerializer.Serialize(q);
                    byte[] questionData = Encoding.UTF8.GetBytes(questionJson + "\n");
                    stream.Write(questionData, 0, questionData.Length);

                    Console.WriteLine($"Question sent: {q.Text}");

                    // Чекаємо на відповідь від клієнта
                    byte[] buffer = new byte[1024];
                    int count = stream.Read(buffer, 0, buffer.Length);

                    if (count == 0) break;

                    string answerJson = Encoding.UTF8.GetString(buffer, 0, count).Trim();
                    var answer = JsonSerializer.Deserialize<Answer>(answerJson);

                    // Перевіряємо відповідь
                    bool isCorrect = answer?.PlayerAnswer?.Trim() == q.Correct?.Trim();

                    Result result = new Result
                    {
                        IsCorrect = isCorrect,
                        CorrectAnswer = q.Correct,
                        Message = isCorrect ? "Correct!" : "Wrong!"
                    };

                    // Відправляємо результат
                    string resultJson = JsonSerializer.Serialize(result);
                    byte[] resultData = Encoding.UTF8.GetBytes(resultJson + "\n");
                    stream.Write(resultData, 0, resultData.Length);

                    Console.WriteLine($"Player answered: {answer?.PlayerAnswer}, Result: {result.Message}");

                    Thread.Sleep(2000); // Пауза перед наступним питанням
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                players.Remove(player);
            }
        }

        // ----------------------------
        static async Task<Question> GetQuestion()
        {
            List<int> categories = new List<int>() { 9, 10, 11, 13, 17, 21 };
            int ID = categories[new Random().Next(categories.Count)];

            string response = await http.GetStringAsync($"https://opentdb.com/api.php?amount=1&category={ID}&difficulty=easy&type=multiple");
            var js = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response);

            string? text = System.Net.WebUtility.HtmlDecode(js?.results[0]?.question?.ToString());
            string? correct = System.Net.WebUtility.HtmlDecode(js?.results[0]?.correct_answer?.ToString());
            List<string> answers = new() { correct };

            foreach (var a in js?.results[0]?.incorrect_answers)
                answers.Add(System.Net.WebUtility.HtmlDecode(a.ToString()));

            answers = answers.OrderBy(a => Guid.NewGuid()).ToList();

            return new Question()
            {
                Text = text,
                Correct = correct,
                Options = answers
            };
        }
    }
}
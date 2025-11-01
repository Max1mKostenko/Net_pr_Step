using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp54
{
    internal class Program
    {
        static HttpClient client = new HttpClient();

        static async Task<string> GetMovie()
        {
            try
            {
                string[] ids = { "tt0111161", "tt0068646", "tt0468569" };
                string id = ids[new Random().Next(ids.Length)];

                string json = await client.GetStringAsync($"http://www.omdbapi.com/?i={id}&apikey=trilogy");

                string title = GetValue(json, "Title");
                string year = GetValue(json, "Year");
                string genre = GetValue(json, "Genre");

                return $"<h2>Фильм</h2><p><b>{title}</b> ({year})<br>Жанр: {genre}</p>";
            }
            catch
            {
                return "<p>Ошибка загрузки фильма</p>";
            }
        }

        static async Task<string> GetQuote()
        {
            try
            {
                string json = await client.GetStringAsync("https://zenquotes.io/api/random");

                json = json.Trim('[', ']');

                string quote = GetValue(json, "q");
                string author = GetValue(json, "a");

                return $"<h2>Цитата</h2><p>\"{quote}\"<br>— {author}</p>";
            }
            catch (Exception ex)
            {
                return $"<p>Ошибка загрузки цитаты: {ex.Message}</p>";
            }
        }

        static string GetValue(string json, string key)
        {
            string search = $"\"{key}\":\"";
            int start = json.IndexOf(search) + search.Length;
            int end = json.IndexOf("\"", start);
            return json.Substring(start, end - start);
        }

        static async Task ManageClient(HttpListenerContext context)
        {
            Console.WriteLine("Клиент подключился");

            string movie = await GetMovie();
            string quote = await GetQuote();

            string html = $@"
<html>
<head>
    <meta charset='utf-8'>
    <title>hw</title>
    <style>
        body {{ font-family: Arial; background: #f0f0f0; padding: 20px; }}
        div {{ max-width: 600px; margin: 0 auto; background: white;
               padding: 20px }}
        h1 {{ color: #ff6b6b; }}
        h2 {{ color: #4ecdc4; }}
    </style>
</head>
<body>
    <div>
        {movie}
        <hr>
        {quote}
    </div>
</body>
</html>";

            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
        }

        static async Task Main(string[] args)
        {
            HttpListener server = new HttpListener();
            server.Prefixes.Add("http://localhost:8080/");

            try
            {
                server.Start();
                Console.WriteLine("Сервер запущен: http://localhost:8080/");

                while (true)
                {
                    var context = await server.GetContextAsync();
                    await ManageClient(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (server.IsListening)
                {
                    server.Stop();
                    server.Close();
                }
            }
        }
    }
}
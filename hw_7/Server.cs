using System.Net;
using System.Text;
using System.Web;
namespace ConsoleApp53
{
    internal class Program
    {
        static HttpClient client = new HttpClient();
        // ------------------------------------------
        static async Task Main(string[] args)
        {
            HttpListener server = new HttpListener();
            server.Prefixes.Add("http://127.0.0.1:8080/");
            try
            {
                server.Start();
                Console.WriteLine("Waiting for connection...");
                while (true)
                {
                    HttpListenerContext newClient = await server.GetContextAsync();
                    _ = Task.Run(() => ManageClient(newClient));
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            finally { server.Stop(); }
        }
        // ------------------------------------------
        static async Task ManageClient(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath.ToLower();
            string response = "";
            switch (path)
            {
                case "/":
                    response = @"
                        <html>
                            <a href='/rate'>Курс валют</a>
                            <a href='/time'>Поточний час</a>
                            <a href='/'>На головну</a>
                        </html>";
                    break;
                case "/rate":
                    response = await GetRate();
                    break;
                case "/time":
                    response = GetTime();
                    break;
                default:
                    response = "<html>✋✖ 404 PAGE NOT FOUND</html>";
                    break;
            }
            await SendResponse(context, response);
        }
        // ------------------------------------------
        static async Task<string> GetRate()
        {
            string result = "";
            try
            {
                string url = "https://api.privatbank.ua/p24api/pubinfo?exchange&coursid=5";
                var response = await client.GetStringAsync(url);
                var js = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response);
                var EUR = js?[0].sale;
                var USD = js?[1].sale;
                result = $"EUR: {EUR} | USD: {USD}";
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return "<html><h1>" + result + "<h1><a href='/'>На головну</a></html>";
        }
        // ------------------------------------------
        static string GetTime()
        {
            string currentTime = DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy");
            return $"<html><h1>Поточний час: {currentTime}</h1><a href='/'>На головну</a></html>";
        }
        // ------------------------------------------
        static async Task SendResponse(HttpListenerContext context, string response)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
    }
}
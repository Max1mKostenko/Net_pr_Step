using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static HttpClient client = new HttpClient();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TabControl_SelectiononChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void Search(object sender, RoutedEventArgs e)
        {
            string movieName = film_in.Text;
            if (string.IsNullOrEmpty(movieName))
            {
                MessageBox.Show("Enter movie!");
                return;
            }

            await Film(movieName);
        }

        async Task Film(string movieName)
        {
            try
            {
                string url = $"https://www.omdbapi.com/?t={movieName}&apikey=9bd4e80";
                var response = await client.GetStringAsync(url);
                var js = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response);

                if (js["Response"] == "False")
                {
                    MessageBox.Show("Movie not found!");
                    return;
                }

                var title = js?.Title;
                film.Text = $"Title: {title}";

                var year = js?.Year;
                publication.Text = $"Year: {year}";

                var rating = js?.imdbRating;
                rate.Text = $"Rating: {rating}";

                var desc = js?.Plot;
                description.Text = $"Description: {desc}";

                string image = js?.Poster;
                if (!string.IsNullOrEmpty(image) && image != "N/A")
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(image);
                    img.EndInit();
                    film_image.Source = img;
                }
                else
                {
                    film_image.Source = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
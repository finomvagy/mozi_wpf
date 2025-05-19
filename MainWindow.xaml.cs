using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:3000") };
        private List<Movie> movies = new List<Movie>();

        public MainWindow()
        {
            InitializeComponent();
            LoadMovies();
        }

        private async void LoadMovies()
        {
            var res = await client.GetAsync("/movies");
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                movies = JsonConvert.DeserializeObject<List<Movie>>(json);
                MovieList.ItemsSource = movies;
            }
        }

        private async void MovieList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MovieList.SelectedItem is Movie selectedMovie)
            {
                var res = await client.GetAsync($"/movies/{selectedMovie.Id}");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var movie = JsonConvert.DeserializeObject<Movie>(json);
                    TitleText.Text = movie.Title;
                    YearText.Text = movie.Year.ToString();
                    DescriptionText.Text = movie.Description;
                }
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var title = TitleInput.Text;
            var description = DescriptionInput.Text;
            if (!int.TryParse(YearInput.Text, out int year)) return;

            var newMovie = new Movie { Title = title, Description = description, Year = year };
            var content = new StringContent(JsonConvert.SerializeObject(newMovie));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.Add("X-Admin", "true");
            var res = await client.PostAsync("/movies", content);
            if (res.IsSuccessStatusCode)
            {
                LoadMovies();
                TitleInput.Text = "";
                DescriptionInput.Text = "";
                YearInput.Text = "";
            }
        }
    }

    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
    }
}
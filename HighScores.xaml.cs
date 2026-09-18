using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for HighScores.xaml
    /// </summary>
    public partial class HighScores : Window
    {
        public HighScores()
        {
            InitializeComponent();
            ReadScores();
            DisplayScores();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public static List<Score> ReadScores()
        {
            List<Score>? scores = null;

            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string asteroidsFolder = Path.Combine(appDataFolder, "Radish");
            asteroidsFolder = Path.Combine(asteroidsFolder, "Asteroids");
            string filePath = Path.Combine(asteroidsFolder, "highscores.json");

            try
            {
                string json = File.ReadAllText(filePath);
                scores = JsonSerializer.Deserialize<List<Score>>(json);
                if (scores == null)
                {
                    scores = MakeNewScores();
                }
            }
            catch (JsonException)
            {
                scores = MakeNewScores();
            }
            catch (IOException)
            {
                scores = MakeNewScores();
            }

            return scores;
        }

        private static List<Score> MakeNewScores()
        {
            List<Score> scores = new List<Score>();
            for (int i = 0; i < 10; i++)
            {
                Score oneScore = new Score();
                oneScore.Rank = i + 1;
                oneScore.HighScore = -1;
                oneScore.DateOfScore = DateTime.MinValue;
                scores.Add(oneScore);
            }
            return scores;
        }

        private void DisplayScores()
        {
            List<Score> scores = ReadScores();

            for (int i = 0; i < scores.Count; i++)
            {
                TextBlock tb = new TextBlock();
                if (scores[i].HighScore == -1)
                    tb.Text = "—";
                else
                    tb.Text = scores[i].Rank.ToString();
                tb.FontSize = 16;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetRow(tb, i + 2);
                Grid.SetColumn(tb, 0);
                MyGrid.Children.Add(tb);

                tb = new TextBlock();
                if (scores[i].HighScore == -1)
                    tb.Text = "—";
                else
                    tb.Text = scores[i].HighScore.ToString("N0");
                tb.FontSize = 16;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetRow(tb, i + 2);
                Grid.SetColumn(tb, 1);
                MyGrid.Children.Add(tb);

                tb = new TextBlock();
                if (scores[i].HighScore == -1)
                    tb.Text = "—";
                else
                    tb.Text = scores[i].DateOfScore.ToString("d");
                tb.FontSize = 16;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetRow(tb, i + 2);
                Grid.SetColumn(tb, 2);
                MyGrid.Children.Add(tb);
            }
        }
    }

    public class Score
    {
        public int Rank { get; set; }
        public int HighScore { get; set; }
        public DateTime DateOfScore { get; set; }
    }
}

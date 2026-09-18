using System.Windows;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for GameOver.xaml
    /// </summary>
    public partial class GameOver : Window
    {
        public GameOver(int score)
        {
            InitializeComponent();
            ScoreText.Text = score.ToString("N0");
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}

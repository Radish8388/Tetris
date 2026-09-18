using System.Windows;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            VolumeSlider.Value = Properties.Settings.Default.EffectsVolume;
            LevelSlider.Value = Properties.Settings.Default.StartingLevel;
            TetrominosSlider.Value = Properties.Settings.Default.NumberOfPieces;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.EffectsVolume = (int)VolumeSlider.Value;
            Properties.Settings.Default.StartingLevel = (int)LevelSlider.Value;
            Properties.Settings.Default.NumberOfPieces = (int)TetrominosSlider.Value;
            Properties.Settings.Default.Save();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VolumeText.Text = VolumeSlider.Value.ToString();
        }

        private void LevelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LevelText.Text = LevelSlider.Value.ToString();
        }

        private void TetrominosSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TetrominosText.Text = TetrominosSlider.Value.ToString();
        }
    }
}

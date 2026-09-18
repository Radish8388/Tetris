using System.Windows;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class Help : Window
    {
        public Help()
        {
            InitializeComponent();
            ShowHelpCheckBox.IsChecked = Properties.Settings.Default.ShowHelp;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowHelp = ShowHelpCheckBox.IsChecked == true;
            Properties.Settings.Default.Save();
            DialogResult = true;
        }
    }
}

using System.Windows;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for Abandon.xaml
    /// </summary>
    public partial class Abandon : Window
    {
        public Abandon()
        {
            InitializeComponent();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

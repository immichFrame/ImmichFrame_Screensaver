using System.IO;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace ImmichFrame_Screensaver
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            UrlTextBox.Text = Settings.CurrentSettings.SavedUrl;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.CurrentSettings.SavedUrl = UrlTextBox.Text;
            Close();
        }
    }
}

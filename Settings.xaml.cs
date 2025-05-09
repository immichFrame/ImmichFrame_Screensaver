using System.IO;
using System.Windows;

namespace ImmichFrame_Screensaver
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            if (File.Exists(Path.Combine(MainWindow.settingsPath, "Settings.json")))
            {
                var strUrl = File.ReadAllText(Path.Combine(MainWindow.settingsPath, "Settings.json"));
                if (!string.IsNullOrWhiteSpace(strUrl))
                {
                    UrlTextBox.Text = strUrl;
                }
            }               
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(MainWindow.settingsPath))
            {
                Directory.CreateDirectory(MainWindow.settingsPath);
            }
            if (!string.IsNullOrWhiteSpace(UrlTextBox.Text))
            {
                File.WriteAllText(Path.Combine(MainWindow.settingsPath, "Settings.json"), UrlTextBox.Text);
                Close();
            }
        }
    }
}

using System.Configuration;
using System.Data;
using System.Windows;


namespace ImmichFrame_Screensaver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (e.Args.Length > 0)
            {
                string arg = e.Args[0].Trim().ToLower();
                if(arg.StartsWith("/s"))
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                }
                else if (arg.StartsWith("/c"))
                {
                    var settings = new Settings();
                    settings.ShowDialog();
                    Shutdown();
                }
                else
                {
                    Shutdown();
                }
            }
        }
    }       

}

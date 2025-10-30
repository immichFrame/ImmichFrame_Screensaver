using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ImmichFrame_Screensaver
{
    public partial class MainWindow : Window
    {
        private NativeOverlayWindow _nativeOverlay;
        public MainWindow()
        {
            InitializeComponent();
            Cursor = System.Windows.Input.Cursors.None;
            LoadWebView();
            Loaded += async (s, e) =>
            {
                await Task.Delay(3000); // 3 second delay
                                        // Start the native overlay in a new thread so it doesn't block the UI
                _ = Task.Run(() =>
                {
                    using (_nativeOverlay = new NativeOverlayWindow())
                    {
                        _nativeOverlay.RunMessageLoop();
                    }
                });
            };
        }
        private async void LoadWebView()
        {
            try
            {
                string userDataFolder = Path.Combine(Settings.appDataFolder, "WebView2");
                Directory.CreateDirectory(userDataFolder);

                var options = new CoreWebView2EnvironmentOptions("--allow-insecure-localhost --allow-running-insecure-content");
                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
                await WebView.EnsureCoreWebView2Async(env);

                // Use SavedUrl if valid, else fallback to default
                string defaultUrl = "https://github.com/immichFrame/ImmichFrame/raw/main/design/16x9%20frame.svg";
                string url = !string.IsNullOrWhiteSpace(Settings.CurrentSettings.SavedUrl)
                    ? Settings.CurrentSettings.SavedUrl
                    : defaultUrl;

                // Validate URL
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
                {
                    MessageBox.Show("Invalid URL in settings. Using placeholder.");
                    uriResult = new Uri(defaultUrl);
                }

                WebView.Source = uriResult;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize WebView2: " + ex.Message);
                CloseScreensaver();
            }
        }

        private void CloseScreensaver()
        {
            Application.Current.Shutdown();
        }
    }
}
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace ImmichFrame_Screensaver
{
    public partial class MainWindow : Window
    {
        //public static string settingsPath = @"C:\ProgramData\ImmichFrame_Screensaver\Settings.json";
        public static string settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\ImmichFrame_Screensaver\";
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        // Declare the hook procedure delegate with the correct signature and make it public
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        // Hook handles
        private IntPtr _keyboardHookHandle = IntPtr.Zero;
        private IntPtr _mouseHookHandle = IntPtr.Zero;

        // Delegate instances
        private HookProc _keyboardProc;
        private HookProc _mouseProc;

        private bool _inputDetectionEnabled = false;
        public MainWindow()
        {
            InitializeComponent();
            Cursor = System.Windows.Input.Cursors.None;
            LoadWebView();
            StartInputDelayAsync();
            _keyboardProc = new HookProc(KeyboardHookProc);
            _mouseProc = new HookProc(MouseHookProc);
        }
        private async void StartInputDelayAsync()
        {
            _inputDetectionEnabled = false; // Disable input detection initially
            await Task.Delay(3000); // Delay for 2 seconds
            _inputDetectionEnabled = true; // Enable input detection
            SetUpHooks();
        }
        private void OpenSettings()
        {
            var settings = new Settings();
            settings.ShowDialog();
        }
        private void LoadWebView()
        {
            if (File.Exists(Path.Combine(MainWindow.settingsPath, "Settings.json")))
            {
                var strUrl = File.ReadAllText(Path.Combine(MainWindow.settingsPath, "Settings.json"));
                if (!string.IsNullOrWhiteSpace(strUrl))
                {
                    WebView.Source = new Uri(strUrl);
                }
                else
                {
                    MessageBox.Show("Please configure settings!");
                    CloseScreensaver();
                }
            }
            else
            {
                WebView.Source = new Uri("https://github.com/immichFrame/ImmichFrame/raw/main/design/16x9%20frame.svg");
            }
        }
        private void SetUpHooks()
        {
            // Set the keyboard hook
            _keyboardHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, IntPtr.Zero, 0);
            // Set the mouse hook
            _mouseHookHandle = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, IntPtr.Zero, 0);
        }

        private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (_inputDetectionEnabled && nCode >= 0)
            {
                CloseScreensaver();
            }
            return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }

        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (_inputDetectionEnabled && nCode >= 0)
            {
                CloseScreensaver();
            }
            return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
        }


        private void CloseScreensaver()
        {
            Application.Current.Shutdown();
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hmod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(IntPtr idHook);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        // Ensure hooks are removed when the window is closed
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            UnhookWindowsHookEx(_keyboardHookHandle);
            UnhookWindowsHookEx(_mouseHookHandle);
        }
    }
}
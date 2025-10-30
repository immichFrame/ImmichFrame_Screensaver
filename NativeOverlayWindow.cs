using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class NativeOverlayWindow : IDisposable
{
    private IntPtr _hwnd;
    private WndProcDelegate _wndProcDelegate;
    private const int WS_EX_LAYERED = 0x80000;
    private const int WS_EX_TRANSPARENT = 0x20;
    private const int WS_EX_TOPMOST = 0x8;
    private const int WS_POPUP = unchecked((int)0x80000000);
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_TOUCH = 0x0240;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_MOUSEMOVE = 0x0200;
    private int? _lastMouseX = null;
    private int? _lastMouseY = null;
    public NativeOverlayWindow()
    {
        _wndProcDelegate = new WndProcDelegate(WndProc);

        var hInstance = Process.GetCurrentProcess().Handle;
        var className = "NativeOverlayWindowClass";

        WNDCLASS wndClass = new WNDCLASS
        {
            lpfnWndProc = _wndProcDelegate,
            hInstance = hInstance,
            lpszClassName = className
        };
        RegisterClass(ref wndClass);

        _hwnd = CreateWindowEx(
            WS_EX_LAYERED | WS_EX_TOPMOST,
            className,
            "",
            WS_POPUP,
            0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height,
            IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

        SetLayeredWindowAttributes(_hwnd, 0, 1, 0x2); // 1 = almost fully transparent
        RegisterTouchWindow(_hwnd, 0);
        ShowWindow(_hwnd, 1); // SW_SHOWNORMAL
        ShowCursor(false);
    }

    public void RunMessageLoop()
    {
        MSG msg;
        while (GetMessage(out msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_MOUSEMOVE)
        {
            int x = (short)(lParam.ToInt32() & 0xFFFF);
            int y = (short)((lParam.ToInt32() >> 16) & 0xFFFF);

            if (_lastMouseX == null || _lastMouseY == null)
            {
                _lastMouseX = x;
                _lastMouseY = y;
                return DefWindowProc(hWnd, msg, wParam, lParam);
            }
            if (_lastMouseX == x && _lastMouseY == y)
            {
                // Mouse hasn't actually moved
                return DefWindowProc(hWnd, msg, wParam, lParam);
            }
            // Mouse has moved, fall through to close
        }

        if (msg == WM_LBUTTONDOWN || msg == WM_TOUCH || msg == WM_KEYDOWN || msg == WM_MOUSEMOVE)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.Application.Current.Shutdown();
            });
            return IntPtr.Zero;
        }
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public void Dispose()
    {
        ShowCursor(true); // Restore cursor
        if (_hwnd != IntPtr.Zero)
        {
            DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }
    }

    // Win32 API declarations
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct WNDCLASS
    {
        public uint style;
        public WndProcDelegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public System.Drawing.Point pt;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern ushort RegisterClass([In] ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        int dwExStyle, string lpClassName, string lpWindowName,
        int dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

    [DllImport("user32.dll")]
    private static extern int ShowCursor(bool bShow);

    [DllImport("user32.dll")]
    private static extern bool RegisterTouchWindow(IntPtr hWnd, uint ulFlags);
}
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace PPC.Helpers
{
    public static class WindowHelper
    {
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private const int GWL_STYLE = -16,
            WS_MAXIMIZEBOX = 0x10000,
            WS_MINIMIZEBOX = 0x20000;


        public static void HideMinimizeAndMaximizeButtons(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
        }
    }
}

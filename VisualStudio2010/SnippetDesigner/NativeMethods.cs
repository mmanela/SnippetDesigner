// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.SnippetDesigner
{
    [ComVisible(true)]
    internal class NativeMethods
    {
        internal const int WM_KEYDOWN = 0x0100,
        WM_CHAR = 0x0102,
        WM_SYSKEYDOWN = 0x0104,
        WM_SYSCHAR = 0x0106,
        WM_SETFOCUS = 0x0007,
        WM_KILLFOCUS = 0x0008;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool IsChild(IntPtr hwndParent, IntPtr hwndChildTest);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr GetFocus();

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndBefore, int x, int y, int cx, int cy, int flags);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // We need this Win32 function to find out if the shell has the focus.
        [DllImport("user32.Dll")]
        internal static extern int GetActiveWindow();
    }
}

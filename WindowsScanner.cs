using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public static class WindowScanner
{
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);


    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    public static List<(IntPtr hWnd, string Titulo)> ObterJanelas(Process processo)
    {
        var resultado = new List<(IntPtr, string)>();
        EnumWindows((hWnd, lParam) =>
        {
            GetWindowThreadProcessId(hWnd, out uint pid);
            if (pid == processo.Id && IsWindowVisible(hWnd))
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                string titulo = sb.ToString();
                if (!string.IsNullOrWhiteSpace(titulo))
                {
                    resultado.Add((hWnd, titulo));
                }
            }
            return true;
        }, IntPtr.Zero);

        return resultado;
    }
    public static bool FocarJanela(IntPtr hWnd)
    {
        return SetForegroundWindow(hWnd);
    }
}
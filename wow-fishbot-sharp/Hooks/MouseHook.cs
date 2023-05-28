﻿using System.Runtime.InteropServices;

namespace wow_fishbot_sharp.Hooks
{
    internal sealed class MouseHook
    {
        private const int WH_MOUSE_LL = 14;
        private const IntPtr WM_MOUSEMOVE = 0x0200;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private readonly LowLevelMouseProc _hookProcDelegate;
        private static IntPtr _hookId = IntPtr.Zero;

        public event EventHandler<string>? OnCursorMove;

        public MouseHook()
        {
            _hookProcDelegate = HookCallback;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == WM_MOUSEMOVE)
                {
                    int x = Marshal.ReadInt32(lParam);
                    int y = Marshal.ReadInt32(lParam + 4);

                    OnCursorMove?.Invoke(this, $"[MouseHook]: Mouse moved to X:{x}, Y:{y}");
                }
                else
                {
                    Console.WriteLine($"[MouseHook]: DEBUG: {wParam:X}");
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void HookMouse()
        {
            _hookId = SetHook(_hookProcDelegate);
        }

        public void UnHookMouse()
        {
            UnhookWindowsHookEx(_hookId);
        }

        private IntPtr SetHook(LowLevelMouseProc hookProcDelegate)
        {
            using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            if (curModule != null)
                return SetWindowsHookEx(WH_MOUSE_LL, hookProcDelegate, GetModuleHandle(curModule.ModuleName), 0);
            return IntPtr.Zero;
        }
    }
}

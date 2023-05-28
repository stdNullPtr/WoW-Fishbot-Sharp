﻿using System.Runtime.InteropServices;

namespace wow_fishbot_sharp.Hooks
{
    internal sealed class WinEventHook
    {
        private const uint EVENT_OBJECT_NAMECHANGE = 0x800C;
        private const uint WINEVENT_OUTOFCONTEXT = 0x0000;
        private const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
        private const long OBJID_CURSOR = 0xFFFFFFF7;

        private delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, long idObject, long idChild, uint dwEventThread, uint dwmsEventTime);
        private readonly WinEventProc _hookProcDelegate;
        private static IntPtr _hookId = IntPtr.Zero;

        public event EventHandler<string>? OnCursorIconChange;

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        public WinEventHook()
        {
            _hookProcDelegate = HookCallback;
        }

        private void HookCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, long idObject, long idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (idObject == OBJID_CURSOR)
            {
                if (eventType == EVENT_OBJECT_NAMECHANGE)
                {
                    OnCursorIconChange?.Invoke(this, "[WinEventHook]: Cursor changed!");
                }
            }
        }

        public void HookWinEvent(uint processId)
        {
            _hookId = SetHook(processId, _hookProcDelegate);
        }

        public void UnHookWinEvent()
        {
            UnhookWinEvent(_hookId);
        }

        private IntPtr SetHook(uint processId, WinEventProc hookProcDelegate)
        {
            return SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, hookProcDelegate, processId, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
        }

    }
}

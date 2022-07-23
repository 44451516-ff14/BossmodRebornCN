﻿using Dalamud.Game.ClientState.Keys;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BossMod.AI
{
    class Broadcast
    {
        private AIConfig _config;
        private List<(VirtualKey, bool)> _broadcasts = new();

        public Broadcast()
        {
            _config = Service.Config.Get<AIConfig>();
            _broadcasts.Add((VirtualKey.F10, false)); // target focus
            _broadcasts.Add((VirtualKey.T, false)); // target target's target
            _broadcasts.Add((VirtualKey.G, false)); // mount
            _broadcasts.Add((VirtualKey.SPACE, false)); // jump
            _broadcasts.Add((VirtualKey.NUMPAD0, false)); // interact
            _broadcasts.Add((VirtualKey.NUMPAD2, false)); // menu navigation
            _broadcasts.Add((VirtualKey.NUMPAD4, false)); // menu navigation
            _broadcasts.Add((VirtualKey.NUMPAD6, false)); // menu navigation
            _broadcasts.Add((VirtualKey.NUMPAD8, false)); // menu navigation
        }

        public void Update()
        {
            if (!_config.BroadcastToSlaves)
                return;

            for (int i = 0; i < _broadcasts.Count; ++i)
            {
                var vk = _broadcasts[i].Item1;
                bool pressed = Service.KeyState[vk];
                if (pressed != _broadcasts[i].Item2)
                {
                    foreach (var w in EnumerateSlaves())
                    {
                        Service.Log($"Broadcast: {vk} to {w}");
                        PostMessageW(w, pressed ? 0x0100u : 0x0101u, (ulong)vk, 0);
                    }
                    _broadcasts[i] = (vk, pressed);
                }
            }
        }

        public static void BroadcastKeypress(VirtualKey vk)
        {
            foreach (var w in EnumerateSlaves())
                SendKeypress(w, vk);
        }

        private static void SendKeypress(IntPtr hwnd, VirtualKey vk)
        {
            PostMessageW(hwnd, 0x0100, (ulong)vk, 0);
            PostMessageW(hwnd, 0x0101, (ulong)vk, 0);
        }

        private static List<IntPtr> EnumerateSlaves()
        {
            List<IntPtr> res = new();
            var active = GetActiveWindow();
            var name = WindowName(active);
            if (name.Length == 0)
                return res;
            EnumWindows((hwnd, lparam) => {
                if (hwnd != active && !IsIconic(hwnd) && WindowName(hwnd) == name)
                    res.Add(hwnd);
                return true;
            }, IntPtr.Zero);
            return res;
        }

        private unsafe static string WindowName(IntPtr hwnd)
        {
            int size = GetWindowTextLengthW(hwnd);
            if (size <= 0)
                return "";

            var buffer = new char[size + 1];
            fixed (char* pbuf = &buffer[0])
            {
                GetWindowTextW(hwnd, pbuf, size + 1);
                return new(pbuf);
            }
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        private unsafe static extern int GetWindowTextW(IntPtr hWnd, char* strText, int maxCount);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern int GetWindowTextLengthW(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern bool PostMessageW(IntPtr hWnd, uint msg, ulong wparam, ulong lparam);
    }
}

#region File Information
//-----------------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="David Rudie">
//     Copyright (C) 2013 David Rudie
//
//     This program is free software; you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation; either version 3 of the License, or
//     (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program; if not, write to the Free Software
//     Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02111, USA.
// </copyright>
//-----------------------------------------------------------------------------
#endregion

namespace Snip
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    /// <summary>
    /// This class holds unsafe native methods.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern short GetAsyncKeyState(
            [In] int keyInt);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(
            [In] string className,
            [In] string windowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(
            [In] IntPtr windowHandle,
            [Out] StringBuilder windowText,
            [In] int maxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(
            [In] IntPtr windowHandle,
            [In] uint message,
            [In] IntPtr wParam,
            [In] IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(
            [In] IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenProcess(
            [In] uint desiredAccess,
            [In] bool inheritHandle,
            [In] int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetProcessWorkingSetSize(
            [In] IntPtr process,
            [In] UIntPtr minimumWorkingSetSize,
            [In] UIntPtr maximumWorkingSetSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcessHeap();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool HeapLock(
            [In] IntPtr heap);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint HeapCompact(
            [In] IntPtr heap,
            [In] uint flags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool HeapUnlock(
            [In] IntPtr heap);

        /// <summary>
        /// Contains a list of window messages to be used with SendMessage().
        /// </summary>
        internal class WindowMessage
        {
            /// <summary>
            /// Notifies a window that the user generated an application command event, for example, by clicking an application command button using the mouse or typing an application command key on the keyboard.
            /// </summary>
            internal const uint WM_APPCOMMAND = 0x319;
        }
    }
}
#region File Information
/*
 * Copyright (C) 2012-2018 David Rudie
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02111, USA.
 */
#endregion

namespace Winter
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class UnsafeNativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateToolhelp32Snapshot(
            [In] Enumerations.Snapshots dwFlags,
            [In] uint idth32ProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool Process32First(
            [In] IntPtr hSnapshot,
            [In] [Out] ref ProcessFunctions.ProcessEntry lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool Process32Next(
            [In] IntPtr hSnapshot,
            [In] [Out] ref ProcessFunctions.ProcessEntry lppe);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterHotKey(
            [In] [Optional] IntPtr windowHandle,
            [In] int id,
            [In] uint modifier,
            [In] uint key);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnregisterHotKey(
            [In] [Optional] IntPtr windowHandle,
            [In] int id);

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
        internal static extern bool IsWow64Process(
            [In] IntPtr process,
            [Out] [MarshalAs(UnmanagedType.Bool)] out bool systemInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(
            [In] IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenProcess(
            [In] Enumerations.ProcessAccess desiredAccess,
            [In] [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
            [In] int processId);

        [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int GetModuleBaseName(
            [In] IntPtr process,
            [In] [Optional] IntPtr moduleHandle,
            [Out] StringBuilder baseName,
            [In] int size);

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetModuleInformation(
            [In] IntPtr process,
            [In] [Optional] IntPtr moduleHandle,
            [Out] IntPtr moduleInfo, // out ModuleInfo moduleInfo
            [In] int size);

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumProcessModulesEx(
            [In] IntPtr process,
            [Out] IntPtr[] moduleHandles,
            [In] int size,
            [Out] out int requiredSize,
            [In] Enumerations.ModuleFilter filterFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReadProcessMemory(
            [In] IntPtr process,
            [In] IntPtr baseAddress,
            [Out] byte[] buffer,
            [In] IntPtr size,
            [Out] IntPtr bytesRead);
    }
}

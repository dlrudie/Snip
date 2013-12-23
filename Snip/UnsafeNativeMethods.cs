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

namespace Winter
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
    }
}
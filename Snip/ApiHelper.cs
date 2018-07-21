#region File Information
/*
 * Copyright (C) 2006 Michael Schierl
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

namespace ManagedWindowsFunctions
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Helper class that contains static methods useful for API programming. This
    /// class is not exposed to the user.
    /// </summary>
    internal static class ApiHelper
    {
        /// <summary>
        /// Throw a <see cref="Win32Exception"/> if the supplied (return) value is zero.
        /// This exception uses the last Win32 error code as error message.
        /// </summary>
        /// <param name="returnValue">The return value to test.</param>
        /// <returns>This will return the return value unless it is 0.</returns>
        /*
        internal static int FailIfZero(int returnValue)
        {
            if (returnValue == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnValue;
        }
        */

        /// <summary>
        /// Throw a <see cref="Win32Exception"/> if the supplied (return) value is zero.
        /// This exception uses the last Win32 error code as error message.
        /// </summary>
        /// <param name="returnValue">The return value to test.</param>
        /// <returns>This will return the return value unless it is 0.</returns>
        internal static IntPtr FailIfZero(IntPtr returnValue)
        {
            if (returnValue == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnValue;
        }
    }
}
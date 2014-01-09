#region File Information
/*
 * Copyright (C) 2012-2014 David Rudie
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
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    /// Because StyleCop.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isNewProcess = false;

            Mutex mutex = null;

            try
            {
                mutex = new Mutex(true, Application.ProductName, out isNewProcess);

                if (isNewProcess)
                {
                    Application.Run(new Snip());
                    mutex.ReleaseMutex();
                }
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.Close();
                }
            }
            // else
            // {
            //     MessageBox.Show("Another instance of " + Application.ProductName + " is already running.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            // }
        }
    }
}

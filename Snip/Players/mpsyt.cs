#region File Information
/*
 * Copyright (C) 2012-2017 David Rudie
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
    using System.Diagnostics;
    using System.Threading;

    internal sealed class mpsyt : MediaPlayer
    {
        public override void Update()
        {

            Process[] processes = Process.GetProcessesByName("mpsyt");

            if (processes.Length > 0)
            {
                string windowTitle = string.Empty;

                foreach (Process process in processes)
                {
                    windowTitle = process.MainWindowTitle;
                    this.Handle = process.MainWindowHandle;
                }

                // We need to check if the title exists,
                // (if mpsyt was ran from console then it whon't exist)
                if (windowTitle.Length > 0)
                {
                    // Remove the '- mpsyt' part of the window title to receive the song title
                    int lastHyphen = windowTitle.LastIndexOf("-", StringComparison.OrdinalIgnoreCase);

                    //If we find a hyphen we can asume that a track is playing
                    if (lastHyphen > 0)
                    {
                        string songTitle = windowTitle.Substring(0, lastHyphen).Trim();

                        if (Globals.SaveAlbumArtwork)
                        {
                            this.SaveBlankImage();
                        }

                        TextHandler.UpdateText(songTitle);
                    }
                    else
                    {
                        TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("NoTrackPlaying"));
                    }
                }
                else
                {
                    TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("MpsNoTitleFound"));
                }
            

            }
        }

        public override void Unload()
        {
            base.Unload();
        }

        public override void ChangeToNextTrack()
        {
            //We need to sleep a few ms to wait for the ctrl+alt to be depressed
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)'>', IntPtr.Zero);
        }

        public override void ChangeToPreviousTrack()
        {
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)'<', IntPtr.Zero);
        }

        public override void IncreasePlayerVolume()
        {
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)'0', IntPtr.Zero);
        }

        public override void DecreasePlayerVolume()
        {
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)'9', IntPtr.Zero);
        }

        public override void PlayOrPauseTrack()
        {
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)' ', IntPtr.Zero);
        }

        public override void StopTrack()
        {
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)'q', IntPtr.Zero);
        }
    }
}

#region File Information
/*
 * Copyright (C) 2012-2016 David Rudie
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
    using System.Globalization;

    internal sealed class VLC : MediaPlayer
    {
        public override void Update()
        {
            Process[] processes = Process.GetProcessesByName("vlc");

            if (processes.Length > 0)
            {
                string vlcTitle = string.Empty;

                foreach (Process process in processes)
                {
                    vlcTitle = process.MainWindowTitle;
                }

                // Check for a hyphen in the title. If a hyphen exists then we need to cut all of the text after the last
                // hyphen because that's the "VLC media player" text, which can vary based on language.
                // If no hyphen exists then VLC is not playing anything.
                int lastHyphen = vlcTitle.LastIndexOf("-", StringComparison.OrdinalIgnoreCase);

                if (lastHyphen > 0)
                {
                    vlcTitle = vlcTitle.Substring(0, lastHyphen).Trim();

                    if (Globals.SaveAlbumArtwork)
                    {
                        this.SaveBlankImage();
                    }

                    // Filter file extension
                    int lastDot = vlcTitle.LastIndexOf('.');

                    if (lastDot > 0)
                    {
                        vlcTitle = vlcTitle.Substring(0, lastDot).Trim();
                    }

                    TextHandler.UpdateText(vlcTitle);
                }
                else
                {
                    TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("NoTrackPlaying"));
                }
            }
            else
            {
                if (Globals.SaveAlbumArtwork)
                {
                    this.SaveBlankImage();
                }

                TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("VLCIsNotRunning"));
            }
        }

        public override void Unload()
        {
            base.Unload();
        }

        public override void ChangeToNextTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.NextTrack));
        }

        public override void ChangeToPreviousTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.PreviousTrack));
        }

        public override void IncreasePlayerVolume()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.VolumeUp));
        }

        public override void DecreasePlayerVolume()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.VolumeDown));
        }

        public override void MutePlayerAudio()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.MuteTrack));
        }

        public override void PlayOrPauseTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.PlayPauseTrack));
        }

        public override void StopTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.StopTrack));
        }
    }
}

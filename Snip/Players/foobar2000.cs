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
    using System.Globalization;

    internal sealed class foobar2000 : MediaPlayer
    {
        public override void Update()
        {
            if (!this.Found)
            {
                System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("foobar2000");

                if (processes.Length > 0)
                {
                    this.Handle = processes[0].MainWindowHandle;
                }

                foreach (var process in processes)
                {
                    process.Dispose();
                }

                processes = null;

                this.Found = true;
                this.NotRunning = false;
            }
            else
            {
                // Make sure the process is still valid.
                if (this.Handle != IntPtr.Zero && this.Handle != null)
                {
                    int windowTextLength = UnsafeNativeMethods.GetWindowText(this.Handle, this.Title, this.Title.Capacity);

                    string foobar2000Title = this.Title.ToString();

                    this.Title.Clear();

                    // If the window title length is 0 then the process handle is not valid.
                    if (windowTextLength > 0)
                    {
                        // Only update the system tray text and text file text if the title changes.
                        if (foobar2000Title != this.LastTitle || Globals.RewriteUpdatedOutputFormat)
                        {
                            Globals.RewriteUpdatedOutputFormat = false;

                            if (foobar2000Title.StartsWith("foobar2000", StringComparison.OrdinalIgnoreCase))
                            {
                                TextHandler.UpdateTextAndEmptyFilesMaybe(LocalizedMessages.NoTrackPlaying);
                            }
                            else
                            {
                                // Winamp window titles look like "[%album artist% - ]['['%album%[ CD%discnumber%][ #%tracknumber%]']' ]%title%[ '//' %track artist%]".
                                // Require that the user use ATF and replace the format with something like:
                                // %artist% – %title%
                                string windowTitleFull = System.Text.RegularExpressions.Regex.Replace(foobar2000Title, @"\s+\[foobar2000.+\]", string.Empty);
                                string[] windowTitle = windowTitleFull.Split('–');

                                // Album artwork not supported by foobar2000
                                if (Globals.SaveAlbumArtwork)
                                {
                                    this.SaveBlankImage();
                                }

                                if (windowTitle.Length > 1)
                                {
                                    string artist = windowTitle[0].Trim();
                                    string songTitle = windowTitle[1].Trim();

                                    TextHandler.UpdateText(songTitle, artist);
                                }
                                else
                                {
                                    TextHandler.UpdateText(windowTitle[0].Trim());
                                }
                            }

                            this.LastTitle = foobar2000Title;
                        }
                    }
                    else
                    {
                        if (!this.NotRunning)
                        {
                            if (Globals.SaveAlbumArtwork)
                            {
                                this.SaveBlankImage();
                            }

                            TextHandler.UpdateTextAndEmptyFilesMaybe(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    LocalizedMessages.PlayerIsNotRunning,
                                    LocalizedMessages.foobar2000));

                            this.Found = false;
                            this.NotRunning = true;
                        }
                    }
                }
                else
                {
                    if (!this.NotRunning)
                    {
                        if (Globals.SaveAlbumArtwork)
                        {
                            this.SaveBlankImage();
                        }

                        TextHandler.UpdateTextAndEmptyFilesMaybe(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                LocalizedMessages.PlayerIsNotRunning,
                                LocalizedMessages.foobar2000));

                        this.Found = false;
                        this.NotRunning = true;
                    }
                }
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

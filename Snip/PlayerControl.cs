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

using System;

namespace Winter
{
    public partial class Snip
    {
        private void ChangeToNextTrack()
        {
            if (this.toolStripMenuItemSpotify.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.NextTrack));
            }
            else if (this.toolStripMenuItemItunes.Checked)
            {
                this.itunes.NextTrack();
            }
            else if (this.toolStripMenuItemWinamp.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.winampApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.NextTrack));
            }
            else if (this.toolStripMenuItemFoobar2000.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.foobar2000App.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.NextTrack));
            }
        }

        private void ChangeToPreviousTrack()
        {
            if (this.toolStripMenuItemSpotify.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.PreviousTrack));
            }
            else if (this.toolStripMenuItemItunes.Checked)
            {
                this.itunes.PreviousTrack();
            }
            else if (this.toolStripMenuItemWinamp.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.winampApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.PreviousTrack));
            }
            else if (this.toolStripMenuItemFoobar2000.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.foobar2000App.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.PreviousTrack));
            }
        }

        private void IncreasePlayerVolume()
        {
            if (this.toolStripMenuItemSpotify.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeUp));
            }
            else if (this.toolStripMenuItemItunes.Checked)
            {
                this.itunes.SoundVolume++;
            }
            else if (this.toolStripMenuItemWinamp.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.winampApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeUp));
            }
            else if (this.toolStripMenuItemFoobar2000.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.foobar2000App.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeUp));
            }
        }

        private void DecreasePlayerVolume()
        {
            if (this.toolStripMenuItemSpotify.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeDown));
            }
            else if (this.toolStripMenuItemItunes.Checked)
            {
                this.itunes.SoundVolume--;
            }
            else if (this.toolStripMenuItemWinamp.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.winampApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeDown));
            }
            else if (this.toolStripMenuItemFoobar2000.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.foobar2000App.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeDown));
            }
        }

        private void MutePlayerAudio()
        {
            if (this.toolStripMenuItemSpotify.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.MuteTrack));
            }
            else if (this.toolStripMenuItemItunes.Checked)
            {
                // Not supported by iTunes.
            }
            else if (this.toolStripMenuItemWinamp.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.winampApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.MuteTrack));
            }
            else if (this.toolStripMenuItemFoobar2000.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.foobar2000App.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.MuteTrack));
            }
        }

        private void PlayOrPauseTrack()
        {
            if (this.toolStripMenuItemSpotify.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.PlayPauseTrack));
            }
            else if (this.toolStripMenuItemItunes.Checked)
            {
                this.itunes.Play();
            }
            else if (this.toolStripMenuItemWinamp.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.winampApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.PlayPauseTrack));
            }
            else if (this.toolStripMenuItemFoobar2000.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.foobar2000App.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.PlayPauseTrack));
            }
        }

        private void PauseTrack()
        {
            if (this.toolStripMenuItemSpotify.Checked)
            {
                // Not supported in Spotify.
            }
            else if (this.toolStripMenuItemItunes.Checked)
            {
                this.itunes.Pause();
            }
            else if (this.toolStripMenuItemWinamp.Checked)
            {
                // Not supported in Winamp.
            }
            else if (this.toolStripMenuItemFoobar2000.Checked)
            {
                // Not supported in foobar2000.
            }
        }

        private void StopTrack()
        {
            if (this.toolStripMenuItemSpotify.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.StopTrack));
            }
            else if (this.toolStripMenuItemItunes.Checked)
            {
                this.itunes.Stop();
            }
            else if (this.toolStripMenuItemWinamp.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.winampApp.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.StopTrack));
            }
            else if (this.toolStripMenuItemFoobar2000.Checked)
            {
                UnsafeNativeMethods.SendMessage(this.foobar2000App.Handle, WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)MediaCommands.StopTrack));
            }
        }
    }
}

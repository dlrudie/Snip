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

using System.Windows.Forms;

namespace Winter
{
    public static class KeyBinds
    {
        const int nextTrack = (int)Keys.OemCloseBrackets;      // Default: ]
        const int previousTrack = (int)Keys.OemOpenBrackets;   // Default: [
        const int volumeUp = (int)Keys.Oemplus;                // Default: +
        const int volumeDown = (int)Keys.OemMinus;             // Default: -
        const int playPauseTrack = (int)Keys.Enter;            // Default: Enter
        const int stopTrack = (int)Keys.Back;                  // Default: Backspace
        const int muteTrack = (int)Keys.M;                     // Default: M
        const int pauseTrack = (int)Keys.P;                    // Default: P

        /// <summary>
        /// Gets the key bind used for switching to the next track.
        /// </summary>
        public static int NextTrack
        {
            get
            {
                return nextTrack;
            }
        }

        /// <summary>
        /// Gets the keybind used for switching to the previous track.
        /// </summary>
        public static int PreviousTrack
        {
            get
            {
                return previousTrack;
            }
        }

        /// <summary>
        /// Gets the key bind used for raising the volume.
        /// </summary>
        public static int VolumeUp
        {
            get
            {
                return volumeUp;
            }
        }

        /// <summary>
        /// Gets the key bind used for lowering the volume.
        /// </summary>
        public static int VolumeDown
        {
            get
            {
                return volumeDown;
            }
        }

        /// <summary>
        /// Gets the key bind used to play or pause the track.
        /// </summary>
        public static int PlayPauseTrack
        {
            get
            {
                return playPauseTrack;
            }
        }

        /// <summary>
        /// Gets the key used to stop playing the currently playing track.
        /// </summary>
        public static int StopTrack
        {
            get
            {
                return stopTrack;
            }
        }

        /// <summary>
        /// Gets the key bind used to mute the currently playing track.
        /// </summary>
        public static int MuteTrack
        {
            get
            {
                return muteTrack;
            }
        }

        /// <summary>
        /// Gets the key used to pause playback of the currently playing track.
        /// </summary>
        public static int PauseTrack
        {
            get
            {
                return pauseTrack;
            }
        }
    }
}
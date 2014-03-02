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

        public static int NextTrack
        {
            get
            {
                return nextTrack;
            }
        }

        public static int PreviousTrack
        {
            get
            {
                return previousTrack;
            }
        }

        public static int VolumeUp
        {
            get
            {
                return volumeUp;
            }
        }

        public static int VolumeDown
        {
            get
            {
                return volumeDown;
            }
        }

        public static int PlayPauseTrack
        {
            get
            {
                return playPauseTrack;
            }
        }

        public static int StopTrack
        {
            get
            {
                return stopTrack;
            }
        }

        public static int MuteTrack
        {
            get
            {
                return muteTrack;
            }
        }

        public static int PauseTrack
        {
            get
            {
                return pauseTrack;
            }
        }
    }
}

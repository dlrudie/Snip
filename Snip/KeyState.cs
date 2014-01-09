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
    public partial class Snip
    {
        /// <summary>
        /// This structure will hold the last key state of a hotkey.
        /// </summary>
        private class KeyState
        {
            private int nextTrack;
            private int previousTrack;
            private int volumeUp;
            private int volumeDown;
            private int playPauseTrack;
            private int stopTrack;
            private int muteTrack;
            private int pauseTrack;

            public int NextTrack
            {
                get
                {
                    return this.nextTrack;
                }

                set
                {
                    this.nextTrack = value;
                }
            }

            public int PreviousTrack
            {
                get
                {
                    return this.previousTrack;
                }

                set
                {
                    this.previousTrack = value;
                }
            }

            public int VolumeUp
            {
                get
                {
                    return this.volumeUp;
                }

                set
                {
                    this.volumeUp = value;
                }
            }

            public int VolumeDown
            {
                get
                {
                    return this.volumeDown;
                }

                set
                {
                    this.volumeDown = value;
                }
            }

            public int PlayPauseTrack
            {
                get
                {
                    return this.playPauseTrack;
                }

                set
                {
                    this.playPauseTrack = value;
                }
            }

            public int StopTrack
            {
                get
                {
                    return this.stopTrack;
                }

                set
                {
                    this.stopTrack = value;
                }
            }

            public int MuteTrack
            {
                get
                {
                    return this.muteTrack;
                }

                set
                {
                    this.muteTrack = value;
                }
            }

            public int PauseTrack
            {
                get
                {
                    return this.pauseTrack;
                }

                set
                {
                    this.pauseTrack = value;
                }
            }
        }
    }
}
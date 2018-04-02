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
    using System.Resources;
    using System.Windows.Forms;

    public static class Globals
    {
        #region Fields

        public const string TrackVariable = "$$t";
        public const string ArtistVariable = "$$a";
        public const string AlbumVariable = "$$l";

        // Hidden variables
        public const string NewLineVariable = "$$n";
        public const string TrackIdVariable = "$$i";
        public const string TrackVariableUppercase = "$$ut";
        public const string ArtistVariableUppercase = "$$ua";
        public const string AlbumVariableUppercase = "$$ul";
        public const string TrackVariableLowercase = "$$lt";
        public const string ArtistVariableLowercase = "$$la";
        public const string AlbumVariableLowercase = "$$ll";

        #endregion

        #region Properties

        public static ResourceManager ResourceManager { get; set; }

        public static MediaPlayer CurrentPlayer { get; set; }

        public static NotifyIcon SnipNotifyIcon { get; set; }

        public static MediaPlayerSelection PlayerSelection { get; set; }
        public static bool SaveSeparateFiles { get; set; }
        public static bool SaveAlbumArtwork { get; set; }
        public static bool KeepSpotifyAlbumArtwork { get; set; }
        public static AlbumArtworkResolution ArtworkResolution { get; set; }
        public static bool CacheSpotifyMetadata { get; set; }
        public static bool SaveHistory { get; set; }
        public static bool DisplayTrackPopup { get; set; }
        public static bool EmptyFileIfNoTrackPlaying { get; set; }
        public static bool EnableHotkeys { get; set; }

        public static string DefaultTrackFormat { get; set; }
        public static string DefaultSeparatorFormat { get; set; }
        public static string DefaultArtistFormat { get; set; }
        public static string DefaultAlbumFormat { get; set; }

        public static string TrackFormat { get; set; }
        public static string SeparatorFormat { get; set; }
        public static string ArtistFormat { get; set; }
        public static string AlbumFormat { get; set; }

        public static bool RewriteUpdatedOutputFormat { get; set; }

        #endregion

        #region Enumerations

        public enum AlbumArtworkResolution : int
        {
            None = 0,       // Compatibility
            Small = 120,    // A small thumbnail with the size of 120x120.
            Medium = 300,   // A medium thumbnail with the size of 300x300.
            Large = 640     // A large thumbnail with the size of 640x640.
        }

        public enum MediaPlayerSelection : int
        {
            Spotify = 0,
            iTunes = 1,
            Winamp = 2,
            foobar2000 = 3,
            VLC = 4,
            GPMDP = 5,
            QuodLibet = 6,
            WindowsMediaPlayer = 7
        }

        public enum MediaCommand : int
        {
            None = 0x0,
            PlayPauseTrack = 0xE0000,
            MuteTrack = 0x80000,
            VolumeDown = 0x90000,
            VolumeUp = 0xA0000,
            StopTrack = 0xD0000,
            PreviousTrack = 0xC0000,
            NextTrack = 0xB0000
        }

        public enum WindowMessage : int
        {
            None = 0x0,
            Hotkey = 0x312,
            AppCommand = 0x319
        }

        #endregion
    }
}

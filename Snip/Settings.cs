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
    using System.Reflection;
    using Microsoft.Win32;

    public static class Settings
    {
        public static void Save()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "SOFTWARE\\{0}\\{1}",
                    AssemblyInformation.AssemblyTitle,
                    Assembly.GetExecutingAssembly().GetName().Version.Major));

            registryKey.SetValue("Player",                         (int)Globals.PlayerSelection);
            registryKey.SetValue("Save Separate Files",            Globals.SaveSeparateFiles.ToString());
            registryKey.SetValue("Save Album Artwork",             Globals.SaveAlbumArtwork.ToString());
            registryKey.SetValue("Keep Spotify Album Artwork",     Globals.KeepSpotifyAlbumArtwork.ToString());
            registryKey.SetValue("Album Artwork Resolution",       (int)Globals.ArtworkResolution);
            registryKey.SetValue("Cache Spotify Metadata",         Globals.CacheSpotifyMetadata.ToString());
            registryKey.SetValue("Save History",                   Globals.SaveHistory.ToString());
            registryKey.SetValue("Display Track Popup",            Globals.DisplayTrackPopup.ToString());
            registryKey.SetValue("Empty File If No Track Playing", Globals.EmptyFileIfNoTrackPlaying.ToString());
            registryKey.SetValue("Enable Hotkeys",                 Globals.EnableHotkeys.ToString());

            registryKey.Close();
        }

        public static void Load()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "SOFTWARE\\{0}\\{1}",
                    AssemblyInformation.AssemblyTitle,
                    Assembly.GetExecutingAssembly().GetName().Version.Major));

            if (registryKey != null)
            {
                Globals.PlayerSelection           = (Globals.MediaPlayerSelection)registryKey.GetValue("Player", Globals.MediaPlayerSelection.Spotify);
                Globals.SaveSeparateFiles         = Convert.ToBoolean(registryKey.GetValue("Save Separate Files", false), CultureInfo.InvariantCulture);
                Globals.SaveAlbumArtwork          = Convert.ToBoolean(registryKey.GetValue("Save Album Artwork", false), CultureInfo.InvariantCulture);
                Globals.KeepSpotifyAlbumArtwork   = Convert.ToBoolean(registryKey.GetValue("Keep Spotify Album Artwork", false), CultureInfo.InvariantCulture);
                Globals.ArtworkResolution         = (Globals.AlbumArtworkResolution)registryKey.GetValue("Album Artwork Resolution", Globals.AlbumArtworkResolution.Small);
                Globals.CacheSpotifyMetadata      = Convert.ToBoolean(registryKey.GetValue("Cache Spotify Metadata", true), CultureInfo.InvariantCulture);
                Globals.SaveHistory               = Convert.ToBoolean(registryKey.GetValue("Save History", false), CultureInfo.InvariantCulture);
                Globals.DisplayTrackPopup         = Convert.ToBoolean(registryKey.GetValue("Display Track Popup", true), CultureInfo.InvariantCulture);
                Globals.EmptyFileIfNoTrackPlaying = Convert.ToBoolean(registryKey.GetValue("Empty File If No Track Playing", true), CultureInfo.InvariantCulture);
                Globals.EnableHotkeys             = Convert.ToBoolean(registryKey.GetValue("Enable Hotkeys", true), CultureInfo.InvariantCulture);
                Globals.TrackFormat               = Convert.ToString(registryKey.GetValue("Track Format", Globals.DefaultTrackFormat), CultureInfo.CurrentCulture);
                Globals.SeparatorFormat           = Convert.ToString(registryKey.GetValue("Separator Format", Globals.DefaultSeparatorFormat), CultureInfo.CurrentCulture);
                Globals.ArtistFormat              = Convert.ToString(registryKey.GetValue("Artist Format", Globals.DefaultArtistFormat), CultureInfo.CurrentCulture);
                Globals.AlbumFormat               = Convert.ToString(registryKey.GetValue("Album Format", Globals.DefaultAlbumFormat), CultureInfo.CurrentCulture);

                registryKey.Close();
            }
            else
            {
                Globals.PlayerSelection           = Globals.MediaPlayerSelection.Spotify;
                Globals.SaveSeparateFiles         = false;
                Globals.SaveAlbumArtwork          = false;
                Globals.KeepSpotifyAlbumArtwork   = false;
                Globals.ArtworkResolution         = Globals.AlbumArtworkResolution.Small;
                Globals.CacheSpotifyMetadata      = true;
                Globals.SaveHistory               = false;
                Globals.DisplayTrackPopup         = false;
                Globals.EmptyFileIfNoTrackPlaying = true;
                Globals.EnableHotkeys             = true;
                Globals.TrackFormat               = Globals.DefaultTrackFormat;
                Globals.SeparatorFormat           = Globals.DefaultSeparatorFormat;
                Globals.ArtistFormat              = Globals.DefaultArtistFormat;
                Globals.AlbumFormat               = Globals.DefaultAlbumFormat;
            }
        }
    }
}

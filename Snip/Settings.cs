#region File Information
/*
 * Copyright (C) 2012-2015 David Rudie
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
    using Microsoft.Win32;

    public static class Settings
    {
        public static void Save()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "SOFTWARE\\{0}\\{1}\\{2}",
                    AssemblyInformation.AssemblyAuthor,
                    AssemblyInformation.AssemblyTitle,
                    AssemblyInformation.AssemblyVersion));

            registryKey.SetValue("Player", (int)Globals.PlayerSelection);

            if (Globals.SaveSeparateFiles)
            {
                registryKey.SetValue("Save Separate Files", "true");
            }
            else
            {
                registryKey.SetValue("Save Separate Files", "false");
            }

            if (Globals.SaveAlbumArtwork)
            {
                registryKey.SetValue("Save Album Artwork", "true");
            }
            else
            {
                registryKey.SetValue("Save Album Artwork", "false");
            }

            if (Globals.KeepSpotifyAlbumArtwork)
            {
                registryKey.SetValue("Keep Spotify Album Artwork", "true");
            }
            else
            {
                registryKey.SetValue("Keep Spotify Album Artwork", "false");
            }

            registryKey.SetValue("Album Artwork Resolution", (int)Globals.ArtworkResolution);

            if (Globals.SaveHistory)
            {
                registryKey.SetValue("Save History", "true");
            }
            else
            {
                registryKey.SetValue("Save History", "false");
            }

            if (Globals.DisplayTrackPopup)
            {
                registryKey.SetValue("Display Track Popup", "true");
            }
            else
            {
                registryKey.SetValue("Display Track Popup", "false");
            }

            if (Globals.EmptyFileIfNoTrackPlaying)
            {
                registryKey.SetValue("Empty File If No Track Playing", "true");
            }
            else
            {
                registryKey.SetValue("Empty File If No Track Playing", "false");
            }

            if (Globals.EnableHotkeys)
            {
                registryKey.SetValue("Enable Hotkeys", "true");
            }
            else
            {
                registryKey.SetValue("Enable Hotkeys", "false");
            }

            registryKey.Close();
        }

        public static void Load()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "SOFTWARE\\{0}\\{1}\\{2}",
                    AssemblyInformation.AssemblyAuthor,
                    AssemblyInformation.AssemblyTitle,
                    AssemblyInformation.AssemblyVersion));

            if (registryKey != null)
            {
                Globals.PlayerSelection = (Globals.MediaPlayerSelection)registryKey.GetValue("Player", Globals.MediaPlayerSelection.Spotify);

                bool saveSeparateFilesChecked = Convert.ToBoolean(registryKey.GetValue("Save Separate Files", false), CultureInfo.InvariantCulture);
                if (saveSeparateFilesChecked)
                {
                    Globals.SaveSeparateFiles = true;
                }
                else
                {
                    Globals.SaveSeparateFiles = false;
                }

                bool saveAlbumArtworkChecked = Convert.ToBoolean(registryKey.GetValue("Save Album Artwork", false), CultureInfo.InvariantCulture);
                if (saveAlbumArtworkChecked)
                {
                    Globals.SaveAlbumArtwork = true;
                }
                else
                {
                    Globals.SaveAlbumArtwork = false;
                }

                bool keepSpotifyAlbumArtwork = Convert.ToBoolean(registryKey.GetValue("Keep Spotify Album Artwork", false), CultureInfo.InvariantCulture);
                if (keepSpotifyAlbumArtwork)
                {
                    Globals.KeepSpotifyAlbumArtwork = true;
                }
                else
                {
                    Globals.KeepSpotifyAlbumArtwork = false;
                }

                Globals.ArtworkResolution = (Globals.AlbumArtworkResolution)registryKey.GetValue("Album Artwork Resolution", Globals.AlbumArtworkResolution.Tiny);

                bool saveHistoryChecked = Convert.ToBoolean(registryKey.GetValue("Save History", false), CultureInfo.InvariantCulture);
                if (saveHistoryChecked)
                {
                    Globals.SaveHistory = true;
                }
                else
                {
                    Globals.SaveHistory = false;
                }

                bool displayTrackPopupChecked = Convert.ToBoolean(registryKey.GetValue("Display Track Popup", true), CultureInfo.InvariantCulture);
                if (displayTrackPopupChecked)
                {
                    Globals.DisplayTrackPopup = true;
                }
                else
                {
                    Globals.DisplayTrackPopup = false;
                }

                bool emptyFileIfNoTrackPlayingChecked = Convert.ToBoolean(registryKey.GetValue("Empty File If No Track Playing", true), CultureInfo.InvariantCulture);
                if (emptyFileIfNoTrackPlayingChecked)
                {
                    Globals.EmptyFileIfNoTrackPlaying = true;
                }
                else
                {
                    Globals.EmptyFileIfNoTrackPlaying = false;
                }

                bool enableHotkeysChecked = Convert.ToBoolean(registryKey.GetValue("Enable Hotkeys", true), CultureInfo.InvariantCulture);
                if (enableHotkeysChecked)
                {
                    Globals.EnableHotkeys = true;
                }
                else
                {
                    Globals.EnableHotkeys = false;
                }

                Globals.TrackFormat = Convert.ToString(registryKey.GetValue("Track Format", Globals.DefaultTrackFormat), CultureInfo.CurrentCulture);

                Globals.SeparatorFormat = Convert.ToString(registryKey.GetValue("Separator Format", Globals.DefaultSeparatorFormat), CultureInfo.CurrentCulture);

                Globals.ArtistFormat = Convert.ToString(registryKey.GetValue("Artist Format", Globals.DefaultArtistFormat), CultureInfo.CurrentCulture);

                Globals.AlbumFormat = Convert.ToString(registryKey.GetValue("Album Format", Globals.DefaultAlbumFormat), CultureInfo.CurrentCulture);

                registryKey.Close();
            }
            else
            {
                Globals.PlayerSelection = Globals.MediaPlayerSelection.Spotify;
                Globals.SaveSeparateFiles = false;
                Globals.SaveAlbumArtwork = false;
                Globals.KeepSpotifyAlbumArtwork = false;
                Globals.ArtworkResolution = Globals.AlbumArtworkResolution.Tiny;
                Globals.SaveHistory = false;
                Globals.DisplayTrackPopup = false;
                Globals.EmptyFileIfNoTrackPlaying = true;
                Globals.EnableHotkeys = true;
                Globals.TrackFormat = Globals.DefaultTrackFormat;
                Globals.SeparatorFormat = Globals.DefaultSeparatorFormat;
                Globals.ArtistFormat = Globals.DefaultArtistFormat;
                Globals.AlbumFormat = Globals.DefaultAlbumFormat;
            }
        }
    }
}

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
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;

    public static class TextHandler
    {
        private static string lastTextToWrite = string.Empty;

        // This will set the notify icon text correctly if it's over 64 characters long, and truncate it if it's over 128.
        public static void SetNotifyIconText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Trim(); //trim trailing spaces

                int maxLength = 127; // 128 max length

                if (text.Length >= maxLength)
                {
                    maxLength -= 4; //need to ensure space to append " ..." to the end of the string

                    //LastIndexOf will search backwards from the specified index, this means that
                    //we search maxLength + 1 characters to find the specified char (space)
                    //this index gets treated as a length (from index 0) in text.Substring,
                    //which implicitly removes the space from the resultant string 
                    int nextSpace = text.LastIndexOf(' ', maxLength); 

                    text = string.Format(CultureInfo.CurrentCulture, "{0} ...", text.Substring(0, (nextSpace > 0) ? nextSpace : maxLength).Trim());
                }

                Type t = typeof(NotifyIcon);

                BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;

                t.GetField("text", hidden).SetValue(Globals.SnipNotifyIcon, text);

                if ((bool)t.GetField("added", hidden).GetValue(Globals.SnipNotifyIcon))
                {
                    t.GetMethod("UpdateIcon", hidden).Invoke(Globals.SnipNotifyIcon, new object[] { true });
                }
            }
        }

        public static void UpdateTextAndEmptyFilesMaybe(string text)
        {
            if (text != lastTextToWrite)
            {
                lastTextToWrite = text;

                SetNotifyIconText(text);

                if (Globals.EmptyFileIfNoTrackPlaying)
                {
                    File.WriteAllText(@Application.StartupPath + @"\Snip.txt", string.Empty);
                }
                else
                {
                    File.WriteAllText(@Application.StartupPath + @"\Snip.txt", text);
                }

                if (Globals.EmptyFileIfNoTrackPlaying)
                {
                    if (Globals.SaveSeparateFiles)
                    {
                        File.WriteAllText(@Application.StartupPath + @"\Snip_Album.txt", string.Empty);
                        File.WriteAllText(@Application.StartupPath + @"\Snip_Artist.txt", string.Empty);
                        File.WriteAllText(@Application.StartupPath + @"\Snip_Track.txt", string.Empty);
                        File.WriteAllText(@Application.StartupPath + @"\Snip_TrackId.txt", string.Empty);
                    }
                }
            }
        }

        public static void UpdateText(string text)
        {
            if (text != lastTextToWrite)
            {
                lastTextToWrite = text;

                // Set the text that appears on the notify icon.
                SetNotifyIconText(text);

                // Write the song title and artist to a text file.
                File.WriteAllText(@Application.StartupPath + @"\Snip.txt", text);

                // Display a popup message of the track.
                if (Globals.DisplayTrackPopup)
                {
                    Globals.SnipNotifyIcon.ShowBalloonTip(500, "Snip", text, ToolTipIcon.None);
                }
            }
        }

        public static void UpdateText(string title, string artist)
        {
            UpdateText(title, artist, string.Empty, string.Empty, string.Empty);
        }

        public static void UpdateText(string title, string artist, string album)
        {
            UpdateText(title, artist, album, string.Empty, string.Empty);
        }

        public static void UpdateText(string title, string artist, string album, string trackId)
        {
            UpdateText(title, artist, album, trackId, string.Empty);
        }

        public static void UpdateText(string title, string artist, string album, string trackId, string json)
        {
            string output = Globals.TrackFormat + Globals.SeparatorFormat + Globals.ArtistFormat;

            if (!string.IsNullOrEmpty(title))
            {
                output = output.Replace(Globals.TrackVariableUppercase, title.ToUpper(CultureInfo.CurrentCulture));
                output = output.Replace(Globals.TrackVariableLowercase, title.ToLower(CultureInfo.CurrentCulture));
                output = output.Replace(Globals.TrackVariable, title);
            }

            if (!string.IsNullOrEmpty(artist))
            {
                output = output.Replace(Globals.ArtistVariableUppercase, artist.ToUpper(CultureInfo.CurrentCulture));
                output = output.Replace(Globals.ArtistVariableLowercase, artist.ToLower(CultureInfo.CurrentCulture));
                output = output.Replace(Globals.ArtistVariable, artist);
            }

            output = output.Replace(Globals.NewLineVariable, "\r\n");
            output = output.Replace(Globals.TrackIdVariable, trackId);

            if (!string.IsNullOrEmpty(album))
            {
                output = output.Replace(Globals.AlbumVariableUppercase, album.ToUpper(CultureInfo.CurrentCulture));
                output = output.Replace(Globals.AlbumVariableLowercase, album.ToLower(CultureInfo.CurrentCulture));
                output = output.Replace(Globals.AlbumVariable, album);
            }

            if (output != lastTextToWrite)
            {
                lastTextToWrite = output;

                // Set the text that appears on the notify icon.
                SetNotifyIconText(output);

                // Write the song title and artist to a text file.
                File.WriteAllText(@Application.StartupPath + @"\Snip.txt", output);

                // Display a popup message of the track.
                if (Globals.DisplayTrackPopup)
                {
                    Globals.SnipNotifyIcon.ShowBalloonTip(500, "Snip", output, ToolTipIcon.None);
                }

                // Check if we want to save artist and track to separate files.
                if (Globals.SaveSeparateFiles)
                {
                    string artistOutput = Globals.ArtistFormat;
                    if (!string.IsNullOrEmpty(artist))
                    {
                        artistOutput = artistOutput.Replace(Globals.ArtistVariableUppercase, artist.ToUpper(CultureInfo.CurrentCulture));
                        artistOutput = artistOutput.Replace(Globals.ArtistVariableLowercase, artist.ToLower(CultureInfo.CurrentCulture));
                        artistOutput = artistOutput.Replace(Globals.ArtistVariable, artist);
                        File.WriteAllText(@Application.StartupPath + @"\Snip_Artist.txt", artistOutput);
                    }

                    string trackOutput = Globals.TrackFormat;
                    if (!string.IsNullOrEmpty(title))
                    {
                        trackOutput = trackOutput.Replace(Globals.TrackVariableUppercase, title.ToUpper(CultureInfo.CurrentCulture));
                        trackOutput = trackOutput.Replace(Globals.TrackVariableLowercase, title.ToLower(CultureInfo.CurrentCulture));
                        trackOutput = trackOutput.Replace(Globals.TrackVariable, title);
                        File.WriteAllText(@Application.StartupPath + @"\Snip_Track.txt", trackOutput);
                    }

                    string albumOutput = Globals.AlbumFormat;
                    if (!string.IsNullOrEmpty(album))
                    {
                        albumOutput = albumOutput.Replace(Globals.AlbumVariableUppercase, album.ToUpper(CultureInfo.CurrentCulture));
                        albumOutput = albumOutput.Replace(Globals.AlbumVariableLowercase, album.ToLower(CultureInfo.CurrentCulture));
                        albumOutput = albumOutput.Replace(Globals.AlbumVariable, album);
                        File.WriteAllText(@Application.StartupPath + @"\Snip_Album.txt", albumOutput);
                    }

                    File.WriteAllText(@Application.StartupPath + @"\Snip_TrackId.txt", trackId);
                    File.WriteAllText(@Application.StartupPath + @"\Snip_Metadata.json", json);
                }

                // If saving track history is enabled, append that information to a separate file.
                if (Globals.SaveHistory)
                {
                    File.AppendAllText(@Application.StartupPath + @"\Snip_History.txt", output + Environment.NewLine);
                }
            }
        }
    }
}

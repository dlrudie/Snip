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
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
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
            UpdateText(title, artist, string.Empty, string.Empty);
        }

        public static void UpdateText(string title, string artist, string album)
        {
            UpdateText(title, artist, album, string.Empty);
        }

        public static void UpdateText(string title, string artist, string album, string trackId)
        {
            string output = Globals.TrackFormat + Globals.SeparatorFormat + Globals.ArtistFormat;

            output = output.Replace(Globals.TrackVariable, title);
            output = output.Replace(Globals.ArtistVariable, artist);
            output = output.Replace(Globals.NewLineVariable, "\r\n");
            output = output.Replace(Globals.TrackIdVariable, trackId);

            if (!string.IsNullOrEmpty(album))
            {
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
                    File.WriteAllText(@Application.StartupPath + @"\Snip_Artist.txt", Globals.ArtistFormat.Replace(Globals.ArtistVariable, artist));
                    File.WriteAllText(@Application.StartupPath + @"\Snip_Track.txt", Globals.TrackFormat.Replace(Globals.TrackVariable, title));
                    File.WriteAllText(@Application.StartupPath + @"\Snip_Album.txt", Globals.AlbumFormat.Replace(Globals.AlbumVariable, album));
                    File.WriteAllText(@Application.StartupPath + @"\Snip_TrackId.txt", trackId);
                }

                // If saving track history is enabled, append that information to a separate file.
                if (Globals.SaveHistory)
                {
                    File.AppendAllText(@Application.StartupPath + @"\Snip_History.txt", output + Environment.NewLine);
                }
            }
        }

        public static string UnifyTitles(string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                // Spotify's search doesn't like all uppercase letters
                // Let's see how all lowercase fairs
                title = title.ToLowerInvariant();

                // For some unknown reason some versions of Spotify include
                // "Spotify - " before the track information. I doubt this
                // particular string would appear in any sane song title, so
                // let's just remove it.
                title = title.Replace(@"Spotify - ", " ");

                // title = title.Replace(@".", " "); // Causes failed search result from Spotify
                title = title.Replace(@"/", " ");
                title = title.Replace(@"\", " ");
                title = title.Replace(@",", " ");
                // title = title.Replace(@"'", " "); // Causes failed search result from Spotify
                title = title.Replace(@"(", " ");
                title = title.Replace(@")", " ");
                title = title.Replace(@"[", " ");
                title = title.Replace(@"]", " ");
                title = title.Replace(@"!", " ");
                title = title.Replace(@"$", " ");
                title = title.Replace(@"%", " ");
                title = title.Replace(@"&", " ");
                title = title.Replace(@"?", " ");
                title = title.Replace(@":", " ");
                title = title.Replace(@"*", " ");

                title = CompactWhitespace(title);

                return title;
            }
            else
            {
                return string.Empty;
            }
        }

        // http://stackoverflow.com/a/16652252
        private static string CompactWhitespace(string text)
        {
            StringBuilder stringBuilder = new StringBuilder(text);

            CompactWhitespace(stringBuilder);

            return stringBuilder.ToString();
        }

        private static void CompactWhitespace(StringBuilder stringBuilder)
        {
            if (stringBuilder.Length == 0)
            {
                return;
            }

            int start = 0;

            while (start < stringBuilder.Length)
            {
                if (char.IsWhiteSpace(stringBuilder[start]))
                {
                    start++;
                }
                else
                {
                    break;
                }
            }

            if (start == stringBuilder.Length)
            {
                stringBuilder.Length = 0;
                return;
            }

            int end = stringBuilder.Length - 1;

            while (end >= 0)
            {
                if (char.IsWhiteSpace(stringBuilder[end]))
                {
                    end--;
                }
                else
                {
                    break;
                }
            }

            int destination = 0;
            bool previousCharIsWhitespace = false;

            for (int i = start; i <= end; i++)
            {
                if (char.IsWhiteSpace(stringBuilder[i]))
                {
                    if (!previousCharIsWhitespace)
                    {
                        previousCharIsWhitespace = true;
                        stringBuilder[destination] = ' ';
                        destination++;
                    }
                }
                else
                {
                    previousCharIsWhitespace = false;
                    stringBuilder[destination] = stringBuilder[i];
                    destination++;
                }
            }

            stringBuilder.Length = destination;
        }
    }
}

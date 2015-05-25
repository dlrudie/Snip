﻿#region File Information
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
                int maxLength = 127; // 128 max length minus 1

                if (text.Length >= maxLength)
                {
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

        private static void UpdateTextAndEmptyFile(string text)
        {
            if (text != lastTextToWrite)
            {
                lastTextToWrite = text;

                SetNotifyIconText(text);

                File.WriteAllText(@Application.StartupPath + @"\Snip.txt", string.Empty);

                if (Globals.SaveSeparateFiles)
                {
                    File.WriteAllText(@Application.StartupPath + @"\Snip_Album.txt", string.Empty);
                    File.WriteAllText(@Application.StartupPath + @"\Snip_Artist.txt", string.Empty);
                    File.WriteAllText(@Application.StartupPath + @"\Snip_Track.txt", string.Empty);
                }
            }
        }

        public static void UpdateText(string text)
        {
            if (Globals.EmptyFileIfNoTrackPlaying)
            {
                UpdateTextAndEmptyFile(text);
            }
            else
            {
                if (text != lastTextToWrite)
                {
                    lastTextToWrite = text;

                    // Set the text that appears on the notify icon.
                    SetNotifyIconText(text);

                    // Write the song title and artist to a text file.
                    File.WriteAllText(@Application.StartupPath + @"\Snip.txt", text);
                }
            }
        }

        public static void UpdateText(string title, string artist)
        {
            UpdateText(title, artist, string.Empty);
        }

        public static void UpdateText(string title, string artist, string album)
        {
            string output = Globals.TrackFormat + Globals.SeparatorFormat + Globals.ArtistFormat;

            output = output.Replace(Globals.TrackVariable, title);
            output = output.Replace(Globals.ArtistVariable, artist);

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

                // Check if we want to save artist and track to separate files.
                if (Globals.SaveSeparateFiles)
                {
                    File.WriteAllText(@Application.StartupPath + @"\Snip_Artist.txt", Globals.ArtistFormat.Replace(Globals.ArtistVariable, artist));
                    File.WriteAllText(@Application.StartupPath + @"\Snip_Track.txt", Globals.TrackFormat.Replace(Globals.TrackVariable, title));
                    File.WriteAllText(@Application.StartupPath + @"\Snip_Album.txt", Globals.AlbumFormat.Replace(Globals.AlbumVariable, album));
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
                title = title.ToUpper(CultureInfo.InvariantCulture);

                title = title.Replace(@".", string.Empty);
                title = title.Replace(@"/", string.Empty);
                title = title.Replace(@"\", string.Empty);
                title = title.Replace(@",", string.Empty);
                title = title.Replace(@"'", string.Empty);
                title = title.Replace(@"(", string.Empty);
                title = title.Replace(@")", string.Empty);
                title = title.Replace(@"[", string.Empty);
                title = title.Replace(@"]", string.Empty);
                title = title.Replace(@"!", string.Empty);
                title = title.Replace(@"$", string.Empty);
                title = title.Replace(@"%", string.Empty);
                title = title.Replace(@"&", string.Empty);
                title = title.Replace(@"?", string.Empty);
                title = title.Replace(@":", string.Empty);

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

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
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Windows.Forms;
    using System.Web;
    using SimpleJson;

    internal sealed class Spotify : MediaPlayer
    {
        public override void Update()
        {
            if (!this.Found)
            {
                this.Handle = UnsafeNativeMethods.FindWindow("SpotifyMainWindow", null);

                this.Found = true;
                this.NotRunning = false;
            }
            else
            {
                // Make sure the process is still valid.
                if (this.Handle != IntPtr.Zero && this.Handle != null)
                {
                    int windowTextLength = UnsafeNativeMethods.GetWindowText(this.Handle, this.Title, this.Title.Capacity);

                    string spotifyTitle = this.Title.ToString();

                    this.Title.Clear();

                    // If the window title length is 0 then the process handle is not valid.
                    if (windowTextLength > 0)
                    {
                        // Only update the system tray text and text file text if the title changes.
                        if (spotifyTitle != this.LastTitle)
                        {
                            if (spotifyTitle == "Spotify")
                            {
                                if (Globals.SaveAlbumArtwork)
                                {
                                    this.SaveBlankImage();
                                }

                                TextHandler.UpdateTextAndEmptyFile(Globals.ResourceManager.GetString("NoTrackPlaying"));
                            }
                            else
                            {
                                // Spotify window titles look like "Spotify - Artist – Song Title".
                                string windowTitleFull = spotifyTitle.Replace("Spotify - ", string.Empty);
                                string[] windowTitle = windowTitleFull.Split('–');

                                string artist = windowTitle[0].Trim();
                                string songTitle = windowTitle[1].Trim();
                                songTitle = songTitle.Replace(" - Explicit Album Version", string.Empty);

                                if (Globals.SaveAlbumArtwork)
                                {
                                    this.HandleSpotifyAlbumArtwork(artist, songTitle);
                                }

                                TextHandler.UpdateText(songTitle, artist);
                            }

                            this.LastTitle = spotifyTitle;
                        }
                    }
                    else
                    {
                        if (!this.NotRunning)
                        {
                            if (Globals.SaveAlbumArtwork)
                            {
                                this.SaveBlankImage();
                            }

                            TextHandler.UpdateTextAndEmptyFile(Globals.ResourceManager.GetString("SpotifyIsNotRunning"));
                            this.Found = false;
                            this.NotRunning = true;
                        }
                    }
                }
                else
                {
                    if (!this.NotRunning)
                    {
                        if (Globals.SaveAlbumArtwork)
                        {
                            this.SaveBlankImage();
                        }

                        TextHandler.UpdateTextAndEmptyFile(Globals.ResourceManager.GetString("SpotifyIsNotRunning"));
                        this.Found = false;
                        this.NotRunning = true;
                    }
                }
            }
        }

        public override void Unload()
        {
            base.Unload();
        }

        public override void ChangeToNextTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, Globals.WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.NextTrack));
        }

        public override void ChangeToPreviousTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, Globals.WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.PreviousTrack));
        }

        public override void IncreasePlayerVolume()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, Globals.WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.VolumeUp));
        }

        public override void DecreasePlayerVolume()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, Globals.WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.VolumeDown));
        }

        public override void MutePlayerAudio()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, Globals.WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.MuteTrack));
        }

        public override void PlayOrPauseTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, Globals.WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.PlayPauseTrack));
        }

        public override void StopTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, Globals.WindowMessageAppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.StopTrack));
        }

        private void HandleSpotifyAlbumArtwork(string artist, string songTitle)
        {
            string albumId = string.Empty;

            try
            {
                using (WebClient jsonWebClient = new WebClient())
                {
                    try
                    {
                        var json = jsonWebClient.DownloadString(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "http://ws.spotify.com/search/1/track.json?q={0}+{1}",
                                HttpUtility.UrlEncode(artist.Replace(":", string.Empty)),
                                HttpUtility.UrlEncode(songTitle.Replace(":", string.Empty))));

                        if (!string.IsNullOrEmpty(json))
                        {
                            dynamic jsonSummary = SimpleJson.DeserializeObject(json);

                            if (jsonSummary != null)
                            {
                                jsonSummary = SimpleJson.DeserializeObject(jsonSummary["tracks"].ToString());

                                foreach (dynamic jsonTrack in jsonSummary)
                                {
                                    string modifiedTitle = TextHandler.UnifyTitles(songTitle);
                                    string foundTitle = TextHandler.UnifyTitles(jsonTrack.name.ToString());

                                    if (foundTitle == modifiedTitle)
                                    {
                                        dynamic jsonAlbum = SimpleJson.DeserializeObject(jsonTrack["album"].ToString());
                                        albumId = jsonAlbum.href.ToString();

                                        break;
                                    }
                                }

                                if (!string.IsNullOrEmpty(albumId))
                                {
                                    albumId = albumId.Substring(albumId.LastIndexOf(':') + 1);

                                    string artworkDirectory = @Application.StartupPath + @"\SpotifyArtwork";
                                    string artworkImagePath = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}.jpg", artworkDirectory, albumId);

                                    if (Globals.KeepSpotifyAlbumArtwork)
                                    {
                                        FileInfo fileInfo = new FileInfo(artworkImagePath);

                                        if (!Directory.Exists(artworkDirectory))
                                        {
                                            Directory.CreateDirectory(artworkDirectory);
                                        }

                                        if (fileInfo.Exists && fileInfo.Length > 0)
                                        {

                                            fileInfo.CopyTo(this.DefaultArtworkFilePath, true);
                                        }
                                        else
                                        {
                                            DownloadSpotifyAlbumArtwork(albumId, (int)Globals.ArtworkResolution, artworkImagePath);
                                            fileInfo.CopyTo(this.DefaultArtworkFilePath, true);
                                        }
                                    }
                                    else
                                    {
                                        DownloadSpotifyAlbumArtwork(albumId, (int)Globals.ArtworkResolution);
                                    }
                                }
                            }
                        }
                    }
                    catch (WebException)
                    {
                        this.SaveBlankImage();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                this.SaveBlankImage();
            }
        }

        private void DownloadSpotifyAlbumArtwork(string albumId, int albumArtworkResolution, string savePath = null)
        {
            this.SaveBlankImage();

            using (WebClientWithShortTimeout webClient = new WebClientWithShortTimeout())
            {
                try
                {
                    webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";
                    var json = webClient.DownloadString(string.Format(CultureInfo.InvariantCulture, "https://embed.spotify.com/oembed/?url=spotify:album:{0}", albumId));

                    if (!string.IsNullOrEmpty(json))
                    {
                        dynamic jsonSummary = SimpleJson.DeserializeObject(json);

                        string imageUrl = jsonSummary.thumbnail_url.ToString().Replace("cover", string.Format(CultureInfo.InvariantCulture, "{0}", albumArtworkResolution));

                        if (savePath == null)
                        {
                            webClient.DownloadFileAsync(new Uri(imageUrl), this.DefaultArtworkFilePath);
                        }
                        else
                        {
                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadSpotifyFileCompleted);
                            webClient.DownloadFileAsync(new Uri(imageUrl), savePath, savePath);
                        }
                    }
                }
                catch (WebException)
                {
                    this.SaveBlankImage();
                }
            }
        }

        private void DownloadSpotifyFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                try
                {
                    File.Copy((string)e.UserState, this.DefaultArtworkFilePath, true);
                }
                catch (IOException)
                {
                    this.SaveBlankImage();
                }
            }
        }

        private class WebClientWithShortTimeout : WebClient
        {
            // How many seconds before webclient times out and moves on.
            private const int WebClientTimeoutSeconds = 10;

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest webRequest = base.GetWebRequest(address);
                webRequest.Timeout = WebClientTimeoutSeconds * 60 * 1000;
                return webRequest;
            }
        }
    }
}
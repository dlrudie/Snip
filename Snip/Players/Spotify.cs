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
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Web;
    using System.Windows.Forms;
    using SimpleJson;

    internal sealed class Spotify : MediaPlayer
    {
        private string json = string.Empty;
        private bool downloadingJson = false;

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
                        // Only update if the title has actually changed.
                        // This prevents unnecessary calls and downloads.
                        if (spotifyTitle != this.LastTitle || Globals.RewriteUpdatedOutputFormat)
                        {
                            Globals.RewriteUpdatedOutputFormat = false;

                            if (spotifyTitle == "Spotify")
                            {
                                if (Globals.SaveAlbumArtwork)
                                {
                                    this.SaveBlankImage();
                                }

                                TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("NoTrackPlaying"));
                            }
                            else
                            {
                                this.DownloadJson(spotifyTitle);

                                if (!string.IsNullOrEmpty(this.json))
                                {
                                    dynamic jsonSummary = SimpleJson.DeserializeObject(this.json);

                                    if (jsonSummary != null)
                                    {
                                        var numberOfResults = jsonSummary.tracks.total;

                                        if (numberOfResults > 0)
                                        {
                                            jsonSummary = SimpleJson.DeserializeObject(jsonSummary.tracks["items"].ToString());

                                            int mostPopular = SelectTrackByPopularity(jsonSummary, spotifyTitle);

                                            TextHandler.UpdateText(
                                                jsonSummary[mostPopular].name.ToString(),
                                                jsonSummary[mostPopular].artists[0].name.ToString(),
                                                jsonSummary[mostPopular].album.name.ToString(),
                                                jsonSummary[mostPopular].id.ToString());

                                            if (Globals.SaveAlbumArtwork)
                                            {
                                                this.DownloadSpotifyAlbumArtwork(jsonSummary[mostPopular].album);
                                            }
                                        }
                                        else
                                        {
                                            // In the event of an advertisement (or any song that returns 0 results)
                                            // then we'll just write the whole title as a single string instead.
                                            TextHandler.UpdateText(spotifyTitle);
                                        }
                                    }
                                }
                                else
                                {
                                    // For whatever reason the JSON file couldn't download
                                    // In the event this happens we'll just display Spotify's window title as the track
                                    TextHandler.UpdateText(spotifyTitle);
                                }
                            }

                            this.LastTitle = spotifyTitle;
                        }
                    }
                    else
                    {
                        if (!this.NotRunning)
                        {
                            this.ResetSinceSpotifyIsNotRunning();
                        }
                    }
                }
                else
                {
                    if (!this.NotRunning)
                    {
                        this.ResetSinceSpotifyIsNotRunning();
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
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.NextTrack));
        }

        public override void ChangeToPreviousTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.PreviousTrack));
        }

        public override void IncreasePlayerVolume()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.VolumeUp));
        }

        public override void DecreasePlayerVolume()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.VolumeDown));
        }

        public override void MutePlayerAudio()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.MuteTrack));
        }

        public override void PlayOrPauseTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.PlayPauseTrack));
        }

        public override void StopTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.StopTrack));
        }

        private void ResetSinceSpotifyIsNotRunning()
        {
            if (!this.SavedBlankImage)
            {
                if (Globals.SaveAlbumArtwork)
                {
                    this.SaveBlankImage();
                }
            }

            TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("SpotifyIsNotRunning"));

            this.Found = false;
            this.NotRunning = true;
        }

        private void DownloadJson(string spotifyTitle)
        {
            // Prevent redownloading JSON if it's already attempting to
            if (!this.downloadingJson)
            {
                this.downloadingJson = true;

                using (WebClient jsonWebClient = new WebClient())
                {
                    try
                    {
                        // There are certain characters that can cause issues with Spotify's search
                        spotifyTitle = TextHandler.UnifyTitles(spotifyTitle);

                        jsonWebClient.Encoding = System.Text.Encoding.UTF8;

                        var downloadedJson = jsonWebClient.DownloadString(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "https://api.spotify.com/v1/search?q={0}&type=track",
                                HttpUtility.UrlEncode(spotifyTitle)));

                        if (!string.IsNullOrEmpty(downloadedJson))
                        {
                            this.json = downloadedJson;
                        }
                    }
                    catch (WebException)
                    {
                        this.json = string.Empty;
                        this.SaveBlankImage();
                    }
                }

                this.downloadingJson = false;
            }
        }

        private static int SelectTrackByPopularity(dynamic jsonSummary, string windowTitle)
        {
            long highestPopularity = 0;

            int currentKey = 0;
            int keyWithHighestPopularity = 0;

            foreach (dynamic track in jsonSummary)
            {
                if (windowTitle.Contains(track.artists[0].name) && windowTitle.Contains(track.name))
                {
                    if (track.popularity > highestPopularity)
                    {
                        highestPopularity = track.popularity;
                        keyWithHighestPopularity = currentKey;
                    }
                }

                currentKey++;
            }

            return keyWithHighestPopularity;
        }

        private void DownloadSpotifyAlbumArtwork(dynamic jsonSummary)
        {
            string albumId = jsonSummary.id.ToString();

            string artworkDirectory = @Application.StartupPath + @"\SpotifyArtwork";
            string artworkImagePath = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}.jpg", artworkDirectory, albumId);

            if (!Directory.Exists(artworkDirectory))
            {
                Directory.CreateDirectory(artworkDirectory);
            }

            FileInfo fileInfo = new FileInfo(artworkImagePath);

            if (fileInfo.Exists && fileInfo.Length > 0)
            {
                fileInfo.CopyTo(this.DefaultArtworkFilePath, true);
            }
            else
            {
                this.SaveBlankImage();

                using (WebClientWithShortTimeout webClient = new WebClientWithShortTimeout())
                {
                    try
                    {
                        // This assumes that the Spotify image array will always have three results (which in all of my tests it has so far)
                        string imageUrl = string.Empty;

                        switch (Globals.ArtworkResolution)
                        {
                            case Globals.AlbumArtworkResolution.Large:
                                imageUrl = jsonSummary.images[0].url.ToString();
                                break;

                            case Globals.AlbumArtworkResolution.Medium:
                                imageUrl = jsonSummary.images[1].url.ToString();
                                break;

                            case Globals.AlbumArtworkResolution.Tiny:
                                imageUrl = jsonSummary.images[2].url.ToString();
                                break;

                            default:
                                imageUrl = jsonSummary.images[0].url.ToString();
                                break;
                        }

                        webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

                        if (Globals.KeepSpotifyAlbumArtwork)
                        {
                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadSpotifyFileCompleted);
                            webClient.DownloadFileAsync(new Uri(imageUrl), artworkImagePath, artworkImagePath);
                        }
                        else
                        {
                            webClient.DownloadFileAsync(new Uri(imageUrl), this.DefaultArtworkFilePath);
                        }

                        this.SavedBlankImage = false;
                    }
                    catch (WebException)
                    {
                        this.SaveBlankImage();
                    }
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

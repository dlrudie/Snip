#region File Information
/*
 * Copyright (C) 2017-2018 David Rudie
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
    using System.IO;
    using System.Net;
    using System.Timers;
    using SimpleJson;

    using Timer = System.Timers.Timer;

    internal sealed class GPMDP : MediaPlayer, IDisposable
    {
        #region Fields

        private Timer checkPlaybackTimer;

        private string pathToPlaybackJson = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
            @"\Google Play Music Desktop Player\json_store\playback.json";

        private bool gpmdpReset = false;

        #endregion

        #region Methods

        public override void Load()
        {
            base.Load();

            this.checkPlaybackTimer = new Timer(1000);
            this.checkPlaybackTimer.Elapsed += this.CheckPlaybackTimer_Elapsed;
            this.checkPlaybackTimer.AutoReset = true;
            this.checkPlaybackTimer.Enabled = true;
        }

        public override void Unload()
        {
            base.Unload();
            this.checkPlaybackTimer.Stop();
        }

        public void Dispose()
        {
            if (this.checkPlaybackTimer != null)
            {
                this.checkPlaybackTimer.Dispose();
                this.checkPlaybackTimer = null;
            }
        }

        private void CheckPlaybackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // There's really no way of knowing if the player is running or not
            // Just check the JSON file for playback status repeatedly

            // Do nothing if the playback file doesn't exist
            if (File.Exists(this.pathToPlaybackJson))
            {
                string json = string.Empty;

                FileStream fileStream = null;
                try
                {
                    fileStream = File.Open(pathToPlaybackJson, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        fileStream = null;
                        json = streamReader.ReadToEnd();
                    }
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                    }
                }

                dynamic jsonSummary = SimpleJson.DeserializeObject(json);

                if (jsonSummary != null)
                {
                    bool gpmdpPlaying = Convert.ToBoolean(jsonSummary.playing);

                    if (!gpmdpPlaying)
                    {
                        this.ResetSnipSinceGPMDPIsNotPlaying();
                    }
                    else
                    {
                        string trackTitle = jsonSummary.song.title.ToString();
                        string trackArtist = jsonSummary.song.artist.ToString();
                        string trackAlbum = jsonSummary.song.album.ToString();
                        string trackAlbumArt = jsonSummary.song.albumArt.ToString();

                        string lastTrack = trackTitle + trackArtist;

                        if (lastTrack != this.LastTitle || Globals.RewriteUpdatedOutputFormat)
                        {
                            Globals.RewriteUpdatedOutputFormat = false;

                            if (!string.IsNullOrEmpty(trackTitle) && !string.IsNullOrEmpty(trackArtist) && !string.IsNullOrEmpty(trackAlbum))
                            {
                                TextHandler.UpdateText(
                                    trackTitle,
                                    trackArtist,
                                    trackAlbum);
                            }
                            else if (!string.IsNullOrEmpty(trackTitle) && !string.IsNullOrEmpty(trackArtist))
                            {
                                TextHandler.UpdateText(
                                    trackTitle,
                                    trackArtist);
                            }
                            else
                            {
                                // We shouldn't be here...
                            }

                            if (Globals.SaveAlbumArtwork && !string.IsNullOrEmpty(trackAlbumArt))
                            {
                                this.DownloadGPMDPAlbumArtwork(trackAlbumArt);
                            }

                            this.LastTitle = lastTrack;

                            this.gpmdpReset = false;
                        }
                    }
                }
            }
        }

        private void DownloadGPMDPAlbumArtwork(string albumArtAddress)
        {
            using (WebClientWithShortTimeout webClient = new WebClientWithShortTimeout())
            {
                try
                {
                    Uri imageUrl = new Uri(albumArtAddress);

                    webClient.Headers.Add("User-Agent", "Snip/" + AssemblyInformation.AssemblyVersion);

                    webClient.DownloadFileAsync(imageUrl, this.DefaultArtworkFilePath);

                    this.SavedBlankImage = false;
                }
                catch (WebException)
                {
                    this.SaveBlankImage();
                }
            }
        }

        private void DownloadGPMDPFileCompleted(object sender, AsyncCompletedEventArgs e)
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

        private void ResetSnipSinceGPMDPIsNotPlaying()
        {
            if (!this.gpmdpReset)
            {
                // Prevent writing a blank image if we already did
                if (!this.SavedBlankImage)
                {
                    if (Globals.SaveAlbumArtwork)
                    {
                        this.SaveBlankImage();
                    }
                }

                TextHandler.UpdateTextAndEmptyFilesMaybe(LocalizedMessages.NoTrackPlaying);

                this.LastTitle = string.Empty;

                this.gpmdpReset = true;
            }
        }

        #endregion

        #region Classes

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

        #endregion
    }
}

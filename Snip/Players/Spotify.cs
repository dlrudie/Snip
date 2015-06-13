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

// Sections of this code are from:
// https://github.com/ikkentim/Spotify

namespace Winter
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Web;
    using System.Windows.Forms;
    using SimpleJson;

    internal sealed class Spotify : MediaPlayer
    {
        private string oauthToken = string.Empty;
        private string csrfToken = string.Empty;

        private string trackTitle = string.Empty;
        private string trackArtist = string.Empty;
        private string trackAlbum = string.Empty;

        private bool spotifyIsPaused = false;

        private bool spotifyTokensObtained = false;

        public override void Load()
        {
            RunSpotifyWebHelperIfNotRunning();

            //this.SpotifyAvailabilityChanged += Spotify_SpotifyAvailabilityChanged;
            //this.SpotifyPlayStateChanged += Spotify_SpotifyPlayStateChanged;
            this.SpotifyTrackChanged += Spotify_SpotifyTrackChanged;
        }

        public override void Update()
        {
            /*
            // Spotify and the helper are not running but it's available.
            if ((!IsSpotifyRunning || !IsSpotifyWebHelperRunning) && IsSpotifyAvailable)
            {
                this.IsSpotifyAvailable = false;

                if (this.SpotifyAvailabilityChanged != null)
                {
                    this.SpotifyAvailabilityChanged(this, EventArgs.Empty);
                }
            }

            // Spotify and the helper are running but are not available.
            if (IsSpotifyRunning && IsSpotifyWebHelperRunning && !IsSpotifyAvailable)
            {
                this.IsSpotifyAvailable = true;

                if (this.SpotifyAvailabilityChanged != null)
                {
                    this.SpotifyAvailabilityChanged(this, EventArgs.Empty);
                }

                this.UpdateTokens();
            }

            // Spotify is running but the helper is not.
            if (IsSpotifyRunning && !IsSpotifyWebHelperRunning)
            {
                RunSpotifyWebHelperIfNotRunning();

                return;
            }

            // Spotify is not available at all.
            if (!IsSpotifyRunning && !IsSpotifyWebHelperRunning)
            {
                TextHandler.UpdateText(Globals.ResourceManager.GetString("SpotifyIsNotRunning"));

                return;
            }*/

            if (!IsSpotifyWebHelperRunning)
            {
                RunSpotifyWebHelperIfNotRunning();

                return;
            }

            if (!IsSpotifyRunning)
            {
                TextHandler.UpdateText(Globals.ResourceManager.GetString("SpotifyIsNotRunning"));

                return;
            }

            if (!this.spotifyTokensObtained)
            {
                this.spotifyTokensObtained = true;

                this.UpdateTokens();
            }

            string json = this.QueryWebHelperStatus();
            dynamic jsonSummary = SimpleJson.DeserializeObject(json);

            bool playingStatusCurrent = false;

            if (jsonSummary != null)
            {
                playingStatusCurrent = jsonSummary.playing;

                if (playingStatusCurrent == true)
                {
                    this.trackTitle = jsonSummary.track.track_resource.name;
                    this.trackArtist = jsonSummary.track.artist_resource.name;
                    this.trackAlbum = jsonSummary.track.album_resource.name;

                    if (this.SpotifyTrackChanged != null)
                    {
                        this.SpotifyTrackChanged(this, EventArgs.Empty);
                    }

                    if (Globals.SaveAlbumArtwork)
                    {
                        string albumId = jsonSummary.track.album_resource.uri.Replace("spotify:album:", "");
                        SaveAlbumArt(albumId);
                    }
                    else
                    {
                        this.SaveBlankImage();
                    }
                }
                else
                {
                    TextHandler.UpdateText(Globals.ResourceManager.GetString("NoTrackPlaying"));
                }
            }
        }

        private void SaveAlbumArt(string albumId)
        {
            string result = String.Empty;

            Uri requestUri = new Uri("https://api.spotify.com/v1/albums/" + albumId);

            using (WebClient client = new WebClient())
            {
                result = client.DownloadString(requestUri);
            }

            dynamic jsonSummary = SimpleJson.DeserializeObject(result);

            string albumArtUri = String.Empty;
            switch (Globals.ArtworkResolution)
            {
                case Globals.AlbumArtworkResolution.Tiny: case Globals.AlbumArtworkResolution.Small:
                    albumArtUri = jsonSummary.images[2].url;
                    break;

                case Globals.AlbumArtworkResolution.Medium:
                    albumArtUri = jsonSummary.images[1].url;
                    break;

                case Globals.AlbumArtworkResolution.Large:
                    albumArtUri = jsonSummary.images[0].url;
                    break;
            }

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(albumArtUri), this.DefaultArtworkFilePath);
            }
        }

        public override void PlayOrPauseTrack()
        {
            if (IsSpotifyRunning && IsSpotifyWebHelperRunning)
            {
                if (this.spotifyIsPaused)
                {
                    this.QueryWebHelper("remote/pause.json", true, true, "pause=false");

                    this.spotifyIsPaused = false;
                }
                else
                {
                    this.QueryWebHelper("remote/pause.json", true, true, "pause=true");

                    this.spotifyIsPaused = true;
                }
            }
        }

        public override void Unload()
        {
            base.Unload();

            //this.SpotifyAvailabilityChanged -= Spotify_SpotifyAvailabilityChanged;
            //this.SpotifyPlayStateChanged -= Spotify_SpotifyPlayStateChanged;
            this.SpotifyTrackChanged -= Spotify_SpotifyTrackChanged;

            this.spotifyTokensObtained = false;
        }

        //private event EventHandler SpotifyAvailabilityChanged;
        private event EventHandler SpotifyTrackChanged;

        private void Spotify_SpotifyTrackChanged(object sender, EventArgs e)
        {
            TextHandler.UpdateText(this.trackTitle, this.trackArtist, this.trackAlbum);
        }

        private void Spotify_SpotifyAvailabilityChanged(object sender, EventArgs e)
        {
        }

        private bool IsSpotifyAvailable
        {
            get;
            set;
        }

        private static bool IsSpotifyRunning
        {
            get
            {
                return Process.GetProcessesByName("spotify").Length >= 1;
            }
        }

        private static bool IsSpotifyWebHelperRunning
        {
            get
            {
                return Process.GetProcessesByName("SpotifyWebHelper").Length >= 1;
            }
        }

        private void UpdateTokens()
        {
            this.csrfToken = this.GetCFIDToken();
            this.oauthToken = GetOAuthToken();
        }

        private string GetCFIDToken()
        {
            var token = string.Empty;

            string json = this.QueryWebHelper("simplecsrf/token.json", false, false).Replace(@"\", "");

            if (!string.IsNullOrEmpty(json))
            {
                dynamic jsonSummary = SimpleJson.DeserializeObject(json);

                if (jsonSummary != null)
                {
                    token = jsonSummary.token;
                }
            }

            return token;
        }

        private static string GetOAuthToken()
        {
            string json = string.Empty;
            var token = string.Empty;

            using (WebClient webClient = new WebClient())
            {
                json = webClient.DownloadString("http://open.spotify.com/token");
            }

            if (!string.IsNullOrEmpty(json))
            {
                dynamic jsonSummary = SimpleJson.DeserializeObject(json);

                if (jsonSummary != null)
                {
                    token = jsonSummary.t;
                }
            }

            return token;
        }

        private static void RunSpotifyIfNotRunning()
        {
            if (!IsSpotifyRunning)
            {
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Spotify\spotify.exe");
            }
        }

        private static void RunSpotifyWebHelperIfNotRunning()
        {
            if (!IsSpotifyWebHelperRunning)
            {
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Spotify\Data\SpotifyWebHelper.exe");
            }
        }

        private string QueryWebHelperStatus()
        {
            string status = string.Empty;

            string response = this.QueryWebHelper("remote/status.json", true, true);

            if (!string.IsNullOrEmpty(response))
            {
                status = response;
            }

            return status;
        }

        private string QueryWebHelper(string request, bool oauth, bool cfid)
        {
            return this.QueryWebHelper(request, oauth, cfid, string.Empty);
        }

        private string QueryWebHelper(string request, bool oauth, bool cfid, string extraParameters)
        {
            string parameters = string.Empty;

            if (!string.IsNullOrEmpty(extraParameters))
            {
                parameters = string.Format(CultureInfo.InvariantCulture, "?{0}&ref=&cors=&_=" + Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds), extraParameters);
            }
            else
            {
                parameters = "?ref=&cors=&_=" + Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            }

            if (oauth)
            {
                parameters += "&oauth=" + this.oauthToken;
            }

            if (cfid)
            {
                parameters += "&csrf=" + this.csrfToken;
            }

            Uri address = new Uri(@"http://localhost:4380/" + request + parameters);

            string result = string.Empty;

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Set("Origin", "https://embed.spotify.com");

                result = webClient.DownloadString(address);
            }

            return result;
        }
    }
}
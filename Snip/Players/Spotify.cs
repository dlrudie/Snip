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
        private string trackTitleLast = string.Empty;
        private string trackArtist = string.Empty;
        private string trackArtistLast = string.Empty;
        private string trackAlbum = string.Empty;
        private string trackAlbumLast = string.Empty;

        private bool spotifyIsPaused = false;

        public override void Load()
        {
            this.RunSpotifyWebHelperIfNotRunning();

            //this.SpotifyAvailabilityChanged += Spotify_SpotifyAvailabilityChanged;
            //this.SpotifyPlayStateChanged += Spotify_SpotifyPlayStateChanged;
            //this.SpotifyTrackChanged += Spotify_SpotifyTrackChanged;
        }

        public override void Update()
        {
            if ((!IsSpotifyRunning || !IsSpotifyWebHelperRunning) && IsSpotifyAvailable)
            {
                this.IsSpotifyAvailable = false;

                if (this.SpotifyAvailabilityChanged != null)
                {
                    this.SpotifyAvailabilityChanged(this, EventArgs.Empty);
                }
            }

            if (IsSpotifyRunning && IsSpotifyWebHelperRunning && !IsSpotifyAvailable)
            {
                this.IsSpotifyAvailable = true;

                if (this.SpotifyAvailabilityChanged != null)
                {
                    this.SpotifyAvailabilityChanged(this, EventArgs.Empty);
                }

                this.UpdateTokens();
            }

            if (this.IsSpotifyRunning && !this.IsSpotifyWebHelperRunning)
            {
                this.RunSpotifyWebHelperIfNotRunning();

                return;
            }

            if (!this.IsSpotifyAvailable)
            {
                return;
            }

            string json = this.QueryWebHelperStatus();
            dynamic jsonSummary = SimpleJson.DeserializeObject(json);

            if (jsonSummary != null)
            {
                this.trackTitle = jsonSummary.track.track_resource.name;
                this.trackArtist = jsonSummary.track.artist_resource.name;
                this.trackAlbum = jsonSummary.track.album_resource.name;

                if (this.trackTitle != this.trackTitleLast &&
                    this.trackArtist != this.trackArtistLast &&
                    this.trackAlbum != this.trackAlbumLast)
                {
                    this.trackTitleLast = this.trackTitle;
                    this.trackArtistLast = this.trackArtist;
                    this.trackAlbumLast = this.trackAlbum;

                    TextHandler.UpdateText(this.trackTitle, this.trackArtist, this.trackAlbum);
                }
            }
        }

        public override void PlayOrPauseTrack()
        {
            if (this.IsSpotifyRunning && this.IsSpotifyWebHelperRunning)
            {
                string pause = string.Empty;

                if (this.spotifyIsPaused)
                {
                    pause = this.QueryWebHelper("remote/pause.json", true, true, "pause=false");

                    this.spotifyIsPaused = false;
                }
                else
                {
                    pause = this.QueryWebHelper("remote/pause.json", true, true, "pause=true");

                    this.spotifyIsPaused = true;
                }
            }
        }

        public override void Unload()
        {
            base.Unload();

            //this.SpotifyAvailabilityChanged -= Spotify_SpotifyAvailabilityChanged;
            //this.SpotifyPlayStateChanged -= Spotify_SpotifyPlayStateChanged;
            //this.SpotifyTrackChanged -= Spotify_SpotifyTrackChanged;
        }

        private event EventHandler SpotifyAvailabilityChanged;

        private bool IsSpotifyAvailable
        {
            get;
            set;
        }

        private bool IsSpotifyRunning
        {
            get
            {
                return Process.GetProcessesByName("spotify").Length >= 1;
            }
        }

        private bool IsSpotifyWebHelperRunning
        {
            get
            {
                return Process.GetProcessesByName("SpotifyWebHelper").Length >= 1;
            }
        }

        private void UpdateTokens()
        {
            this.csrfToken = this.GetCFIDToken();
            this.oauthToken = this.GetOAuthToken();
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

        private string GetOAuthToken()
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

        private void RunSpotifyIfNotRunning()
        {
            if (!this.IsSpotifyRunning)
            {
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Spotify\spotify.exe");
            }
        }

        private void RunSpotifyWebHelperIfNotRunning()
        {
            if (!this.IsSpotifyWebHelperRunning)
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
                parameters = string.Format("?{0}&ref=&cors=&_=" + Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds), extraParameters);
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
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
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Timers;
    using System.Windows.Forms;
    using SimpleJson;

    using Timer = System.Timers.Timer;

    internal sealed class Spotify : MediaPlayer, IDisposable
    {
        #region Fields

        private Timer updateOAuthTokenTimer;
        private Timer updateCSRFTokenTimer;
        private Timer updateAuthorizationTokenTimer;
        private Timer contactSpotifyLocalServerTimer;

        private string oauthToken = string.Empty;
        private string csrfToken = string.Empty;
        private string authorizationToken = string.Empty;
        private double authorizationTokenExpiration = 0;

        private bool spotifyWindowFound = false;

        private bool snipReset = false;

        private volatile bool spotifyPortDetectionInProgress = false;
        private volatile int spotifyPort = 0;
        private volatile string spotilocalAddress = string.Empty;

        #endregion

        #region Methods

        public override void Load()
        {
            base.Load();

            this.DetectSpotifyWebHelperPort();

            // Retrieve OAuth token from Spotify
            // I'm not sure on how long before this token expires so I'm default it to
            // obtain a new token every hour.
            this.updateOAuthTokenTimer = new Timer(3600 * 1000);
            this.updateOAuthTokenTimer.Elapsed += this.UpdateOAuthTokenTimer_Elapsed;
            this.updateOAuthTokenTimer.AutoReset = true;
            this.updateOAuthTokenTimer.Enabled = true;
            this.UpdateOAuthTokenTimer_Elapsed(null, null); // Get initial token

            // Retrieve CSRF token from local Spotify client
            // I'm not sure on how long before this token expires so I'm default it to
            // obtain a new token every hour.
            this.updateCSRFTokenTimer = new Timer(3600 * 1000);
            this.updateCSRFTokenTimer.Elapsed += this.UpdateCSRFTokenTimer_Elapsed;
            this.updateCSRFTokenTimer.AutoReset = true;
            this.updateCSRFTokenTimer.Enabled = true;
            this.UpdateCSRFTokenTimer_Elapsed(null, null); // Get initial token

            // Set up the authorization token
            // As of 2017 May 29 an authorization token is required for all API endpoints
            // Default to 1 second in the event the token fails to be obtained
            this.updateAuthorizationTokenTimer = new Timer(1000);
            this.updateAuthorizationTokenTimer.Elapsed += this.UpdateAuthorizationTokenTimer_Elapsed;
            this.updateAuthorizationTokenTimer.AutoReset = true;
            this.updateAuthorizationTokenTimer.Enabled = true;
            this.UpdateAuthorizationTokenTimer_Elapsed(null, null); // Get initial token

            // This is the main timer that will gather all of the information from Spotify
            // Set to a second so it updates frequently but not ridiculously
            this.contactSpotifyLocalServerTimer = new Timer(1000);
            this.contactSpotifyLocalServerTimer.Elapsed += this.ContactSpotifyLocalServerTimer_Elapsed;
            this.contactSpotifyLocalServerTimer.AutoReset = true;
            this.contactSpotifyLocalServerTimer.Enabled = true;
        }

        public override void Unload()
        {
            base.Unload();
            this.snipReset = false;
            this.spotifyWindowFound = false;
            this.oauthToken = string.Empty;
            this.csrfToken = string.Empty;
            this.authorizationToken = string.Empty;
            this.authorizationTokenExpiration = 0;
            this.updateOAuthTokenTimer.Stop();
            this.updateCSRFTokenTimer.Stop();
            this.updateAuthorizationTokenTimer.Stop();
            this.contactSpotifyLocalServerTimer.Stop();
            this.spotifyPort = 0;
            this.spotilocalAddress = string.Empty;
        }

        public void Dispose()
        {
            if (this.contactSpotifyLocalServerTimer != null)
            {
                this.contactSpotifyLocalServerTimer.Dispose();
                this.contactSpotifyLocalServerTimer = null;
            }

            if (this.updateAuthorizationTokenTimer != null)
            {
                this.updateAuthorizationTokenTimer.Dispose();
                this.updateAuthorizationTokenTimer = null;
            }

            if (this.updateCSRFTokenTimer != null)
            {
                this.updateCSRFTokenTimer.Dispose();
                this.updateCSRFTokenTimer = null;
            }

            if (this.updateOAuthTokenTimer != null)
            {
                this.updateOAuthTokenTimer.Dispose();
                this.updateOAuthTokenTimer = null;
            }
        }

        private void UpdateOAuthTokenTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string downloadedJson = this.DownloadJson("https://open.spotify.com/token", SpotifyAddressContactType.Default);

            // Set the token to be blank until filled
            this.oauthToken = string.Empty;

            if (!string.IsNullOrEmpty(downloadedJson))
            {
                dynamic jsonSummary = SimpleJson.DeserializeObject(downloadedJson);

                if (jsonSummary != null)
                {
                    this.oauthToken = jsonSummary.t.ToString();
                }
            }
        }

        private void UpdateAuthorizationTokenTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // TODO:
            // Implement retry if token expired (just in case)
            //
            // JSON result if expired:
            // {
            //   "error": {
            //     "status": 401,
            //     "message": "The access token expired"
            //   }
            // }

            string downloadedJson = this.DownloadJson("https://accounts.spotify.com/api/token", SpotifyAddressContactType.Authorization);

            // Set the token to be blank until filled
            this.authorizationToken = string.Empty;
            this.authorizationTokenExpiration = 0;

            if (!string.IsNullOrEmpty(downloadedJson))
            {
                dynamic jsonSummary = SimpleJson.DeserializeObject(downloadedJson);

                if (jsonSummary != null)
                {
                    this.authorizationToken = jsonSummary.access_token.ToString();
                    this.authorizationTokenExpiration = Convert.ToDouble((long)jsonSummary.expires_in);

                    this.updateAuthorizationTokenTimer.Interval = this.authorizationTokenExpiration * 1000.0;
                }
            }
        }

        private void UpdateCSRFTokenTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.spotifyPort > 0)
            {
                // CSRF token path
                string csrfAddress = "/simplecsrf/token.json";

                string downloadedJson = this.DownloadJson(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}:{1}{2}",
                        this.spotilocalAddress,
                        this.spotifyPort,
                        csrfAddress),
                    SpotifyAddressContactType.CSRF);

                // Set the token to be blank until filled
                this.csrfToken = string.Empty;

                if (!string.IsNullOrEmpty(downloadedJson))
                {
                    dynamic jsonSummary = SimpleJson.DeserializeObject(downloadedJson);

                    if (jsonSummary != null)
                    {
                        // If Spotify is running this value will be null
                        if (jsonSummary.running == null)
                        {
                            this.csrfToken = jsonSummary.token.ToString();
                            this.updateCSRFTokenTimer.Interval = 3600 * 1000; // We got what we wanted
                        }
                        else
                        {
                            this.ResetSnipSinceSpotifyIsNotRunning();
                            this.updateCSRFTokenTimer.Interval = 1000; // Run continously until token is obtained
                        }
                    }
                }
            }
            else
            {
                // Let the user know that we're searching for SpotifyWebHelper.
                TextHandler.UpdateTextAndEmptyFilesMaybe(LocalizedMessages.LocatingSpotifyWebHelper);

                this.DetectSpotifyWebHelperPort();
            }
        }

        private void ContactSpotifyLocalServerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string trackInformation = this.GetTrackInformation();

            // We should only be here if Spotify actually is running
            if (!string.IsNullOrEmpty(trackInformation))
            {
                // Get the handle so that hotkeys can be used
                this.GetSpotifyWindowHandle();

                dynamic jsonSummary = SimpleJson.DeserializeObject(trackInformation);

                if (jsonSummary != null)
                {
                    bool spotifyPlaying = Convert.ToBoolean(jsonSummary.playing);

                    if (!spotifyPlaying)
                    {
                        this.ResetSnipSinceSpotifyIsNotPlaying();
                    }
                    else
                    {
                        string trackType = jsonSummary.track.track_type.ToString();

                        if (trackType == "ad")
                        {
                            // Consider supporting Spotify and subscribing
                            this.ResetSnipSinceSpotifyIsNotPlaying();
                        }
                        else if (trackType == "local")
                        {
                            string fullTrackId = jsonSummary.track.track_resource.uri.ToString();
                            string trackId = fullTrackId.Substring(fullTrackId.LastIndexOf(':') + 1); // + 1 to not include :

                            // Only update if the title has changed or the user updates how the output format should look
                            if (trackId != this.LastTitle || Globals.RewriteUpdatedOutputFormat)
                            {
                                Globals.RewriteUpdatedOutputFormat = false;

                                string trackName = jsonSummary.track.track_resource.name.ToString();
                                string artistName = jsonSummary.track.artist_resource.name.ToString();

                                if (!string.IsNullOrEmpty(artistName))
                                {
                                    // If there is a valid ID3 tag the artist should be set
                                    TextHandler.UpdateText(
                                            trackName,
                                            artistName);
                                }
                                else
                                {
                                    // If no ID3 tag exists then the track name defaults to the filename
                                    TextHandler.UpdateText(trackName);
                                }

                                // Set the last title to the track id as these are unique values that only change when the track changes
                                this.LastTitle = trackId;

                                this.snipReset = false;
                            }
                        }
                        else
                        {
                            string fullTrackId = jsonSummary.track.track_resource.uri.ToString();
                            string trackId = fullTrackId.Substring(fullTrackId.LastIndexOf(':') + 1); // + 1 to not include :

                            // Only update if the title has changed or the user updates how the output format should look
                            if (trackId != this.LastTitle || Globals.RewriteUpdatedOutputFormat)
                            {
                                Globals.RewriteUpdatedOutputFormat = false;

                                string json = string.Empty;

                                if (Globals.CacheSpotifyMetadata)
                                {
                                    json = this.ReadCachedJson(trackId);
                                }
                                else
                                {
                                    json = this.DownloadJson(
                                        string.Format(
                                            CultureInfo.InvariantCulture,
                                            "https://api.spotify.com/v1/tracks/{0}",
                                            trackId),
                                        SpotifyAddressContactType.API);
                                }

                                // This shouldn't happen... but you never know.
                                if (!string.IsNullOrEmpty(json))
                                {
                                    jsonSummary = SimpleJson.DeserializeObject(json);

                                    // If there are multiple artists we want to join all of them together for display
                                    string artists = string.Empty;

                                    foreach (dynamic artist in jsonSummary.artists)
                                    {
                                        artists += artist.name.ToString() + ", ";
                                    }

                                    artists = artists.Substring(0, artists.LastIndexOf(',')); // Removes last comma

                                    TextHandler.UpdateText(
                                        jsonSummary.name.ToString(),
                                        artists,
                                        jsonSummary.album.name.ToString(),
                                        jsonSummary.id.ToString(),
                                        jsonSummary.ToString());

                                    if (Globals.SaveAlbumArtwork)
                                    {
                                        this.DownloadSpotifyAlbumArtwork(jsonSummary.album);
                                    }

                                    // Set the last title to the track id as these are unique values that only change when the track changes
                                    this.LastTitle = trackId;

                                    this.snipReset = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DetectSpotifyWebHelperPort()
        {
            if (!this.spotifyPortDetectionInProgress)
            {
                // We're attempting to detect the correct port
                this.spotifyPortDetectionInProgress = true;

                // This thread will set the detection progress to false when complete
                Thread detectSpotifyWebHelperPortThread = new Thread(this.DetectSpotifyWebHelperPortThread);
                detectSpotifyWebHelperPortThread.Start();
            }
        }

        private void DetectSpotifyWebHelperPortThread()
        {
            // No need to repeat finding the port if it's already found
            if (this.spotifyPort <= 0)
            {
                // *.spotilocal.com redirects to localhost
                string localAddress = "://127.0.0.1";

                // After doing some research SpotifyWebHelper uses several ports
                // 4370 - 4379 = https
                // 4380 - 4389 = http
                // However I've only ever personally seen SpotifyWebHelper listen on ports 4370, 4371, 4380, and 4381.

                string versionAddress = "/service/version.json?service=remote";

                for (int port = 4370; port < 4390; port++)
                {
                    string addressPrefix = string.Empty;

                    if (port >= 4370 && port <= 4379)
                    {
                        addressPrefix = "https";
                    }
                    else
                    {
                        addressPrefix = "http";
                    }

                    string downloadedJson = this.DownloadJson(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}:{1}{2}",
                            addressPrefix + localAddress,
                            port,
                            versionAddress),
                        SpotifyAddressContactType.CSRF);

                    if (!string.IsNullOrEmpty(downloadedJson))
                    {
                        dynamic jsonSummary = SimpleJson.DeserializeObject(downloadedJson);

                        if (jsonSummary != null)
                        {
                            if (jsonSummary.version != null)
                            {
                                this.spotifyPort = port;
                                this.spotilocalAddress = addressPrefix + localAddress;
                                break;
                            }
                        }
                    }
                }

                if (this.spotifyPort <= 0)
                {
                    // "We ain't found shit"
                    this.spotilocalAddress = string.Empty;
                }

                // We're done here
                this.spotifyPortDetectionInProgress = false;
            }
        }

        private void GetSpotifyWindowHandle()
        {
            if (!this.spotifyWindowFound)
            {
                this.Handle = UnsafeNativeMethods.FindWindow("Chrome_WidgetWin_0", null);
                if (this.Handle != IntPtr.Zero && this.Handle != null)
                {
                    this.spotifyWindowFound = true;
                }
            }
        }

        private string GetTrackInformation()
        {
            // No sense in doing anything if the tokens aren't valid or set yet
            if (!string.IsNullOrEmpty(this.oauthToken) || !string.IsNullOrEmpty(this.csrfToken))
            {
                if (this.spotifyPort > 0)
                {
                    string csrfAddress = string.Format(
                        CultureInfo.InvariantCulture,
                        "/remote/status.json?oauth={0}&csrf={1}",
                        oauthToken,
                        csrfToken);

                    string downloadedJson = this.DownloadJson(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}:{1}{2}",
                            this.spotilocalAddress,
                            this.spotifyPort,
                            csrfAddress),
                        SpotifyAddressContactType.CSRF);

                    if (!string.IsNullOrEmpty(downloadedJson))
                    {
                        // Check if Spotify is still running
                        dynamic jsonSummary = SimpleJson.DeserializeObject(downloadedJson);

                        // If Spotify is running this value will be null
                        if (Convert.ToBoolean(jsonSummary.running))
                        {
                            return downloadedJson;
                        }
                    }
                }
                else
                {
                    this.DetectSpotifyWebHelperPort();
                }
            }

            // We should only be here if all else failed, which means it probably can't connect to SpotifyWebHelper
            this.ResetSnipSinceSpotifyIsNotRunning();
            this.updateCSRFTokenTimer.Interval = 1000; // Run continously until token is obtained
            return string.Empty;
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

                this.SavedBlankImage = false;
            }
            else
            {
                using (WebClientWithShortTimeout webClient = new WebClientWithShortTimeout())
                {
                    try
                    {
                        Uri imageUrl = SelectAlbumArtworkSizeToDownload(jsonSummary);

                        webClient.Headers.Add("User-Agent", "Snip/" + AssemblyInformation.AssemblyVersion);

                        if (Globals.KeepSpotifyAlbumArtwork)
                        {
                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadSpotifyFileCompleted);
                            webClient.DownloadFileAsync(imageUrl, artworkImagePath, artworkImagePath);
                        }
                        else
                        {
                            webClient.DownloadFileAsync(imageUrl, this.DefaultArtworkFilePath);
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

        private string DownloadJson(string jsonAddress, SpotifyAddressContactType spotifyAddressContactType)
        {
            using (WebClientWithShortTimeout jsonWebClient = new WebClientWithShortTimeout())
            {
                try
                {
                    // Authorization uses POST instead of GET
                    bool usePostMethodInsteadOfGet = false;
                    string postParameters = string.Empty;

                    // Modify HTTP headers based on what's being contacted
                    switch (spotifyAddressContactType)
                    {
                        case SpotifyAddressContactType.Authorization:
                            usePostMethodInsteadOfGet = true;
                            postParameters = "grant_type=client_credentials";
                            jsonWebClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                            jsonWebClient.Headers.Add("Authorization", string.Format(CultureInfo.InvariantCulture, "Basic {0}", ApplicationKeys.Spotify));
                            break;

                        case SpotifyAddressContactType.CSRF:
                            jsonWebClient.Headers.Add("Origin", "https://open.spotify.com");
                            break;

                        case SpotifyAddressContactType.Status:
                            jsonWebClient.Headers.Add("Origin", "https://open.spotify.com");
                            break;

                        case SpotifyAddressContactType.API:
                            jsonWebClient.Headers.Add("Authorization", string.Format(CultureInfo.InvariantCulture, "Bearer {0}", this.authorizationToken));
                            break;

                        default:
                            break;
                    }

                    // Let's be respectful and identify ourself
                    jsonWebClient.Headers.Add("User-Agent", "Snip/" + AssemblyInformation.AssemblyVersion);

                    jsonWebClient.Encoding = Encoding.UTF8;

                    string downloadedJson = string.Empty;
                    if (usePostMethodInsteadOfGet)
                    {
                        downloadedJson = jsonWebClient.UploadString(jsonAddress, "POST", postParameters);
                    }
                    else
                    {
                        downloadedJson = jsonWebClient.DownloadString(jsonAddress);
                    }

                    if (!string.IsNullOrEmpty(downloadedJson))
                    {
                        return downloadedJson;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                catch (WebException)
                {
                    return string.Empty;
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

        private string ReadCachedJson(string trackId)
        {
            string json = string.Empty;

            string metadataDirectory = @Application.StartupPath + @"\SpotifyMetadata";
            string metadataJsonPath = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}.json", metadataDirectory, trackId);

            if (!Directory.Exists(metadataDirectory))
            {
                Directory.CreateDirectory(metadataDirectory);
            }

            if (File.Exists(metadataJsonPath))
            {
                json = File.ReadAllText(metadataJsonPath, Encoding.UTF8);
            }
            else
            {
                json = this.DownloadJson(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "https://api.spotify.com/v1/tracks/{0}",
                        trackId),
                    SpotifyAddressContactType.API);

                if (!string.IsNullOrEmpty(json))
                {
                    File.WriteAllText(metadataJsonPath, json, Encoding.UTF8);
                }
            }

            if (!string.IsNullOrEmpty(json))
            {
                return json;
            }
            else
            {
                return string.Empty;
            }
        }

        private void ResetSnipSinceSpotifyIsNotPlaying()
        {
            if (!this.snipReset)
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

                this.snipReset = true;
            }
        }

        private void ResetSnipSinceSpotifyIsNotRunning()
        {
            if (this.spotifyWindowFound)
            {
                this.spotifyWindowFound = false;
            }

            // Prevent writing a blank image if we already did
            if (!this.SavedBlankImage)
            {
                if (Globals.SaveAlbumArtwork)
                {
                    this.SaveBlankImage();
                }
            }

            TextHandler.UpdateTextAndEmptyFilesMaybe(
                string.Format(
                    CultureInfo.InvariantCulture,
                    LocalizedMessages.PlayerIsNotRunning,
                    LocalizedMessages.Spotify));
        }

        private static Uri SelectAlbumArtworkSizeToDownload(dynamic jsonSummary)
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

                case Globals.AlbumArtworkResolution.Small:
                    imageUrl = jsonSummary.images[2].url.ToString();
                    break;

                default:
                    imageUrl = jsonSummary.images[2].url.ToString();
                    break;
            }

            return new Uri(imageUrl);
        }

        #endregion

        #region Player Control Methods

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

        #endregion

        #region Enumerations

        private enum SpotifyAddressContactType
        {
            Authorization,
            CSRF,
            Status,
            API,
            Default
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

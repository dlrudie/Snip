#region File Information
/*
 * Copyright (C) 2012-2022 David Rudie
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
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Timers;
    using System.Windows.Forms;
    using SimpleJson;

    using Timer = System.Timers.Timer;

    internal sealed class Spotify : MediaPlayer, IDisposable
    {
        #region Fields

        private string authorizationAddress = "https://accounts.spotify.com/authorize";

        private string scopes = "user-read-playback-state user-read-currently-playing user-modify-playback-state";
        private string responseType = "code"; // Required by API
        private HttpListener httpListener;
        private bool httpListenerStop;
        private string callbackAddress = "http://localhost:10597/";

        private string authorizationCode = string.Empty;
        private string authorizationToken = string.Empty;
        private double authorizationTokenExpiration = 0;
        private string refreshToken = string.Empty;

        private Timer updateAuthorizationToken;
        private Timer updateSpotifyTrackInformation;

        private double updateAuthorizationTokenDefaultInterval = 1000;
        private double updateSpotifyTrackInformationDefaultInterval = 2000;

        private bool rateLimited = false;

        #endregion

        #region Methods

        public override void Load()
        {
            base.Load();

            this.AuthorizeSnip();

            // Set up the authorization token
            // Default to 1 second in the event the token fails to be obtained
            this.updateAuthorizationToken = new Timer(updateAuthorizationTokenDefaultInterval);
            this.updateAuthorizationToken.Elapsed += this.UpdateAuthorizationToken_Elapsed;
            this.updateAuthorizationToken.AutoReset = true;
            this.updateAuthorizationToken.Enabled = true;
            this.UpdateAuthorizationToken_Elapsed(null, null); // Get initial token

            // This is the main timer that will gather all of the information from Spotify
            // Set to 3 second so it updates frequently but not ridiculously
            this.updateSpotifyTrackInformation = new Timer(updateSpotifyTrackInformationDefaultInterval);
            this.updateSpotifyTrackInformation.Elapsed += this.UpdateSpotifyTrackInformation_Elapsed;
            this.updateSpotifyTrackInformation.AutoReset = true;
            this.updateSpotifyTrackInformation.Enabled = true;
        }

        public override void Unload()
        {
            base.Unload();
            this.authorizationCode = string.Empty;
            this.authorizationToken = string.Empty;
            this.authorizationTokenExpiration = 0;
            this.refreshToken = string.Empty;
            this.updateAuthorizationToken.Stop();
            this.updateSpotifyTrackInformation.Stop();
        }

        public void Dispose()
        {
            if (this.updateSpotifyTrackInformation != null)
            {
                this.updateSpotifyTrackInformation.Dispose();
                this.updateSpotifyTrackInformation = null;
            }

            if (this.updateAuthorizationToken != null)
            {
                this.updateAuthorizationToken.Dispose();
                this.updateAuthorizationToken = null;
            }
        }

        private async void AuthorizeSnip()
        {
            try
            {
                Process authorizationProcess = Process.Start(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}?client_id={1}&response_type={2}&redirect_uri={3}&scope={4}",
                        this.authorizationAddress,
                        ApplicationKeys.SpotifyClientId,
                        this.responseType,
                        this.callbackAddress,
                        this.scopes
                        ));

                this.httpListener = new HttpListener();
                this.httpListener.Prefixes.Add(this.callbackAddress);

                this.httpListener.Start();

                while (!this.httpListenerStop)
                {
                    HttpListenerContext httpListenerContext = null;

                    try
                    {
                        httpListenerContext = await this.httpListener.GetContextAsync();
                    }
                    catch (HttpListenerException exception)
                    {
                        if (exception.ErrorCode == 995)
                            return;
                    }

                    if (httpListenerContext == null)
                        continue;

                    HttpListenerRequest request = httpListenerContext.Request;
                    HttpListenerResponse response = httpListenerContext.Response;

                    NameValueCollection nameValueCollection = request.QueryString;

                    string callbackHtmlStart = "<!doctype html><html lang=en><head><meta charset=utf-8><style>html, body { height: 100%; margin: 0; padding: 0; } body { background-color: rgb(255, 255, 255); color: rgb(0, 0, 0); } div.wrapper { display: table; table-layout: fixed; width: 100%; height: 90%; } div.container { display: table-cell; vertical-align: middle; } div.centered { font-family: Arial, Helvetica, sans-serif; font-size: 1em; font-weight: normal; text-align: center; }</style><title>Snip</title></head><body><div class=wrapper><div class=container><div class=centered>";
                    string callbackHtmlEnd = "</div></div></div></body></html>";

                    string outputString = string.Empty;

                    foreach (string keyValue in nameValueCollection.AllKeys)
                    {
                        switch (keyValue)
                        {
                            case "error":
                                outputString = string.Format(
                                    CultureInfo.InvariantCulture,
                                    "{0}{1}{2}",
                                    callbackHtmlStart,
                                    "Well... you denied Snip access. Snip cannot work with Spotify until you grant access. To grant access relaunch Snip. Thank you.",
                                    callbackHtmlEnd);
                                break;
                            case "code":
                                outputString = string.Format(
                                    CultureInfo.InvariantCulture,
                                    "{0}{1}{2}",
                                    callbackHtmlStart,
                                    "Snip successfully authorized with Spotify. You may close this window now.",
                                    callbackHtmlEnd);
                                this.authorizationCode = nameValueCollection[keyValue];
                                break;
                            default:
                                break;
                        }
                    }

                    byte[] buffer = Encoding.UTF8.GetBytes(outputString);
                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
            }
            catch
            {
                // add error checking
            }
        }

        private void HttpListenerStop()
        {
            this.httpListenerStop = true;
            this.httpListener.Stop();
        }

        private void UpdateAuthorizationToken_Elapsed(object sender, ElapsedEventArgs e)
        {
            string downloadedJson = string.Empty;

            if (!string.IsNullOrEmpty(this.refreshToken))
            {
                downloadedJson = this.DownloadJson("https://accounts.spotify.com/api/token", SpotifyAddressContactType.AuthorizationRefresh);
            }
            else
            {
                downloadedJson = this.DownloadJson("https://accounts.spotify.com/api/token", SpotifyAddressContactType.Authorization);
            }

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
                    this.refreshToken = jsonSummary.refresh_token.ToString();

                    this.updateAuthorizationToken.Interval = this.authorizationTokenExpiration * 1000.0;
                }
            }
        }

        private void UpdateSpotifyTrackInformation_Elapsed(object sender, ElapsedEventArgs e)
        {
            string downloadedJson = this.DownloadJson("https://api.spotify.com/v1/me/player/currently-playing", SpotifyAddressContactType.API);

            if (!string.IsNullOrEmpty(downloadedJson))
            {
                dynamic jsonSummary = SimpleJson.DeserializeObject(downloadedJson);

                bool isPlaying = (bool)jsonSummary.is_playing;

                // Check if anything is even playing
                if (isPlaying)
                {
                    // track, episode, ad, unknown
                    string currentlyPlayingType = (string)jsonSummary.currently_playing_type;

                    // Spotify does not provide any detailed information for anything other than actual songs.
                    // Podcasts have types of "episode" but do not contain any useful data unfortunately.
                    if (currentlyPlayingType == "track")
                    {
                        bool isLocal = (bool)jsonSummary.item.is_local;

                        string trackId = (string)jsonSummary.item.id;
                        string uri = (string)jsonSummary.item.uri;

                        string comparison;

                        // Use the track ID for comparison if it's from Spotify
                        // Otherwise use the uri as it's unique to the local file
                        if (isLocal)
                        {
                            comparison = uri;
                        }
                        else
                        {
                            comparison = trackId;
                        }

                        // If something is playing, check if we need to do anything else by comparing the track ID
                        // to the track ID from the last update.
                        if (this.LastTitle != comparison)
                        {
                            // The track ID is different so we can continue


                            // If the track ID matches something we've already cached, let's pull that data from
                            // the cache instead of downloading it again.
                            
                            /* This is pointless at the moment since you get all of the metadata when checking
                             * if anything is playing already anyway.
                            if (Globals.CacheSpotifyMetadata)
                            {
                                // Skip if local, there is no metadata
                                if (!isLocal)
                                {
                                    downloadedJson = this.ReadCachedJson(trackId);
                                }
                            }
                            */

                            // If there are multiple artists we want to join all of them together for display
                            string artists = string.Empty;

                            foreach (dynamic artist in jsonSummary.item.artists)
                            {
                                artists += artist.name.ToString() + ", ";
                            }

                            artists = artists.Substring(0, artists.LastIndexOf(',')); // Removes last comma

                            // Update the track being played
                            TextHandler.UpdateText(
                                jsonSummary.item.name.ToString(),
                                artists,
                                jsonSummary.item.album.name.ToString(),
                                trackId,
                                downloadedJson);


                            // If we're saving artwork let's do that now
                            if (Globals.SaveAlbumArtwork)
                            {
                                // Skip if local, no artwork supported yet
                                if (!isLocal)
                                {
                                    this.DownloadSpotifyAlbumArtwork(jsonSummary.item.album);
                                }
                                else
                                {
                                    this.SaveBlankImage();
                                }
                            }

                            // Update LastTitle to reflect the current track ID
                            // We compare this the next time this method gets called
                            this.LastTitle = comparison;
                        }
                    }
                    else
                    {
                        // It's not a track so let's reset
                        this.ResetSnipSinceSpotifyIsNotPlaying();
                    }
                }
                else
                {
                    // If nothing is playing let's reset everything
                    this.ResetSnipSinceSpotifyIsNotPlaying();
                }

                // Reset timer after it was potentially changed by rate limit
                // Since we should only reach this point if valid JSON was obtained this means
                // that the timer shouldn't reset unless there was a success.
                this.updateSpotifyTrackInformation.Enabled = false;
                this.updateSpotifyTrackInformation.Interval = updateSpotifyTrackInformationDefaultInterval;
                this.updateSpotifyTrackInformation.Enabled = true;
                this.rateLimited = false;
            }
            else
            {
                if (this.rateLimited)
                {
                    // If we are rate limited let's just not update or do anything yet. Once there is a successful update
                    // then the information will update accordingly.
                }
                else
                {
                    // If the downloaded JSON is null or empty it's likely because there's no player running
                    this.ResetSnipSinceSpotifyIsNotPlaying();
                }
            }
        }

        private void DownloadSpotifyAlbumArtwork(dynamic jsonSummary)
        {
            string albumId = jsonSummary.id;

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
                            postParameters = string.Format(
                                CultureInfo.InvariantCulture,
                                "grant_type=authorization_code&code={0}&redirect_uri={1}",
                                this.authorizationCode,
                                this.callbackAddress);
                            jsonWebClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                            jsonWebClient.Headers.Add("Authorization", string.Format(CultureInfo.InvariantCulture, "Basic {0}", ApplicationKeys.Spotify));
                            break;

                        case SpotifyAddressContactType.AuthorizationRefresh:
                            usePostMethodInsteadOfGet = true;
                            postParameters = string.Format(
                                CultureInfo.InvariantCulture,
                                "grant_type=refresh_token&refresh_token={0}",
                                this.refreshToken);
                            jsonWebClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                            jsonWebClient.Headers.Add("Authorization", string.Format(CultureInfo.InvariantCulture, "Basic {0}", ApplicationKeys.Spotify));
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
                catch (WebException webException)
                {
                    WebResponse webResponse = webException.Response;
                    WebHeaderCollection webHeaderCollection = webResponse.Headers;

                    for (int i = 0; i < webHeaderCollection.Count; i++)
                    {
                        if (webHeaderCollection.GetKey(i).ToUpperInvariant() == "RETRY-AFTER")
                        {
                            this.rateLimited = true;

                            // Set the timer to the retry seconds. Plus 1 for safety.
                            this.updateSpotifyTrackInformation.Enabled = false;
                            this.updateSpotifyTrackInformation.Interval = (Double.Parse(webHeaderCollection.Get(i) + 1, CultureInfo.InvariantCulture)) * 1000;
                            this.updateSpotifyTrackInformation.Enabled = true;
                        }
                    }

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
            // Prevent writing a blank image if we already did
            if (!this.SavedBlankImage)
            {
                if (Globals.SaveAlbumArtwork)
                {
                    this.SaveBlankImage();
                }
            }

            this.LastTitle = string.Empty;

            TextHandler.UpdateTextAndEmptyFilesMaybe(LocalizedMessages.NoTrackPlaying);
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

        private void SpotifyPlayerControl(SpotifyPlayerControlType controlType)
        {
            try
            {
                using (WebClientWithShortTimeout webClient = new WebClientWithShortTimeout())
                {
                    string urlAddress = "https://api.spotify.com/v1/me/player/";
                    string methodType = string.Empty;

                    switch (controlType)
                    {
                        case SpotifyPlayerControlType.Play:
                            urlAddress += "play";
                            methodType = "PUT";
                            break;
                        case SpotifyPlayerControlType.Pause:
                            urlAddress += "pause";
                            methodType = "PUT";
                            break;
                        case SpotifyPlayerControlType.NextTrack:
                            urlAddress += "next";
                            methodType = "POST";
                            break;
                        case SpotifyPlayerControlType.PreviousTrack:
                            urlAddress += "previous";
                            methodType = "POST";
                            break;
                        default:
                            break;
                    }

                    webClient.Headers.Add("Authorization", string.Format(CultureInfo.InvariantCulture, "Bearer {0}", this.authorizationToken));
                    webClient.Headers.Add("User-Agent", "Snip/" + AssemblyInformation.AssemblyVersion);
                    webClient.Encoding = Encoding.UTF8;

                    webClient.UploadString(urlAddress, methodType, string.Empty);
                }
            }
            catch
            {
                // If you send a request to pause the track, or play the track, when it is already paused or playing
                // it will send a 403 Forbidden. This will silently ignore the exception.
            }
        }

        #endregion

        #region Player Control Methods

        public override void ChangeToNextTrack()
        {
            this.SpotifyPlayerControl(SpotifyPlayerControlType.NextTrack);
        }

        public override void ChangeToPreviousTrack()
        {
            this.SpotifyPlayerControl(SpotifyPlayerControlType.PreviousTrack);
        }

        public override void PlayOrPauseTrack()
        {
            this.SpotifyPlayerControl(SpotifyPlayerControlType.Play);
        }

        public override void PauseTrack()
        {
            this.SpotifyPlayerControl(SpotifyPlayerControlType.Pause);
        }

        #endregion

        #region Enumerations

        private enum SpotifyPlayerControlType
        {
            NextTrack,
            PreviousTrack,
            Pause,
            Play
        }

        private enum SpotifyAddressContactType
        {
            Authorization,
            AuthorizationRefresh,
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

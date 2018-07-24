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

        private Timer updateAuthorizationTokenTimer;
        private Timer updateSpotifyTrackTimer;

        private string authorizationToken = string.Empty;
        private double authorizationTokenExpiration = 0;

        private string exeName = "spotify";
        private string moduleName = "chrome_elf.dll";
        private int processId = 0;
        private int processIdLast = 0;
        private int moduleBaseAddress = 0;
        private int moduleLength = 0;

        // String[36] "spotify:track:1234567890abcdefghijkl"
        private string searchString = "spotify:track:";

        #endregion

        #region Methods

        public override void Load()
        {
            base.Load();

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
            this.updateSpotifyTrackTimer = new Timer(1000);
            this.updateSpotifyTrackTimer.Elapsed += this.UpdateSpotifyTrackTimer_Elapsed;
            this.updateSpotifyTrackTimer.AutoReset = true;
            this.updateSpotifyTrackTimer.Enabled = true;
        }

        public override void Unload()
        {
            base.Unload();
            try
            {
                ProcessFunctions.CloseMemory(this.Handle);
            }
            catch
            {
            }
            this.authorizationToken = string.Empty;
            this.authorizationTokenExpiration = 0;
            this.updateAuthorizationTokenTimer.Stop();
            this.updateSpotifyTrackTimer.Stop();
            this.Handle = IntPtr.Zero;
            this.processId = 0;
            this.processIdLast = 0;
        }

        public void Dispose()
        {
            if (this.updateSpotifyTrackTimer != null)
            {
                this.updateSpotifyTrackTimer.Dispose();
                this.updateSpotifyTrackTimer = null;
            }

            if (this.updateAuthorizationTokenTimer != null)
            {
                this.updateAuthorizationTokenTimer.Dispose();
                this.updateAuthorizationTokenTimer = null;
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

        private void UpdateSpotifyTrackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Locate and detect Spotify and get handles
            this.SetUpSpotifyHandles();

            // If the process ID is greater than 0 then we found the process
            if (this.processId > 0)
            {
                // The track ID is 22 bytes long
                byte[] trackIdInBytes = new byte[22];

                // We can use this to determine if Spotify is playing or not.
                // If Spotify is not playing the titlebar will be "Spotify". Otherwise it
                // will contain the artist and track title.
                string windowTitle = ProcessFunctions.GetProcessWindowTitle(this.processId);

                byte[] searchBytes = Encoding.Default.GetBytes(this.searchString);
                int addressOfTrackId = ProcessFunctions.FindInMemory(this.processId, this.moduleBaseAddress, this.moduleLength, searchBytes);
                // 14 is the offset where the trackId begins, trackId is 22 chars long
                trackIdInBytes = ProcessFunctions.ReadMemory(this.Handle, (IntPtr)addressOfTrackId + 14, 22);

                if (windowTitle == "Spotify")
                {
                    // Because we search for this first there's a brief moment on startup where it may display
                    // that no track is playing before it states that Spotify is not running.
                    this.ResetSnipSinceSpotifyIsNotPlaying();
                }
                else
                {
                    string trackId = Encoding.Default.GetString(trackIdInBytes);

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
                            dynamic jsonSummary = SimpleJson.DeserializeObject(json);

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
                        }
                    }
                }
            }
        }

        private void SetUpSpotifyHandles()
        {
            if (this.processId <= 0 || this.processId != this.processIdLast)
            {
                int someOtherProcessId = ProcessFunctions.GetProcessId(this.exeName);
                this.processId = ProcessFunctions.GetParentProcessId(someOtherProcessId, this.exeName);

                if (this.processId > 0)
                {
                    ProcessFunctions.ModuleInfo moduleInfo = ProcessFunctions.GetModuleInfo(this.processId, this.moduleName);

                    if ((int)moduleInfo.BaseOfDll > 0)
                    {
                        this.Handle = ProcessFunctions.OpenProcess(this.processId, Enumerations.ProcessAccess.VMAll);
                        this.processIdLast = this.processId;
                        this.moduleBaseAddress = (int)moduleInfo.BaseOfDll;
                        this.moduleLength = (int)moduleInfo.SizeOfImage;
                    }
                    else
                    {
                        this.ResetSnipSinceSpotifyIsNotRunning();
                    }
                }
                else
                {
                    this.ResetSnipSinceSpotifyIsNotRunning();
                }
            }

            // Quick check to test if Spotify is running. There is a better solution for this but this will hold everything over for now.
            // It also makes a complaint because process is never actually used.
            try
            {
                Process process = Process.GetProcessById(this.processId);
            }
            catch
            {
                this.ResetSnipSinceSpotifyIsNotRunning();
            }
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
        }

        private void ResetSnipSinceSpotifyIsNotRunning()
        {
            // Prevent writing a blank image if we already did
            if (!this.SavedBlankImage)
            {
                if (Globals.SaveAlbumArtwork)
                {
                    this.SaveBlankImage();
                }
            }

            this.Handle = IntPtr.Zero;
            this.processId = 0;
            this.moduleBaseAddress = 0;
            this.moduleLength = 0;

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

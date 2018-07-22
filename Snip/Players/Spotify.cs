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

using System.Linq;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

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
        private Timer contactSpotifyLocalServerTimer;
        
        private string authorizationToken = string.Empty;
        private double authorizationTokenExpiration = 0;

        private string exeName = "spotify";
        private string moduleName = "chrome_elf.dll";
        private int processId = 0;
        private int processIdLast = 0;
        private int moduleBaseAddress = 0;
        private int moduleLength = 0;

        private bool snipReset = false;

        private volatile bool spotifyPortDetectionInProgress = false;
        private volatile int spotifyPort = 0;
        private volatile string spotilocalAddress = string.Empty;
        private SpotifyWebAPI _spotify;
        
        #endregion

        #region Methods

        public override async void Load()
        {
            base.Load();
            
            var webApiFactory = new WebAPIFactory(
                "http://localhost/",
                8000,
                ApplicationKeys.Spotify, 
                Scope.UserReadRecentlyPlayed,
                TimeSpan.FromSeconds(20)
            );
            
            try
            {
                //This will open the user's browser and returns once
                //the user is authorized.
                _spotify = await webApiFactory.GetWebApi();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (_spotify == null)
            {
                ResetSnipSinceSpotifyIsNotRunning();
                return;
            }
            
            
            // This is the main timer that will gather all of the information from Spotify
            // Set to a second so it updates frequently but not ridiculously
            this.contactSpotifyLocalServerTimer= new Timer(1000);
            this.contactSpotifyLocalServerTimer.Elapsed += this.ContactSpotifyLocalServerTimer_Elapsed;
            this.contactSpotifyLocalServerTimer.AutoReset = true;
            this.contactSpotifyLocalServerTimer.Enabled = true;
        }

        public override void Unload()
        {
            base.Unload();
            this.snipReset = false;
            this.authorizationToken = string.Empty;
            this.authorizationTokenExpiration = 0;
            this.spotifyPort = 0;
            this.spotilocalAddress = string.Empty;
        }

        public void Dispose()
        {
            _spotify?.Dispose();
        }

        private void ContactSpotifyLocalServerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var trackInformation = _spotify.GetPlayingTrack();

            // We should only be here if Spotify actually is running
            if (trackInformation.Error == null)
            {
                var spotifyPlaying = trackInformation.IsPlaying;

                if (!spotifyPlaying)
                {
                    this.ResetSnipSinceSpotifyIsNotPlaying();
                }
                else
                {
                    string fullTrackId = trackInformation.Item.Id;

                    // Only update if the title has changed or the user updates how the output format should look
                    if (fullTrackId != this.LastTitle || Globals.RewriteUpdatedOutputFormat)
                    {
                        Globals.RewriteUpdatedOutputFormat = false;
                            // If there are multiple artists we want to join all of them together for display
                            var artists = string.Join(", ", trackInformation.Item.Artists.Select(x => x.Name));

                            TextHandler.UpdateText(
                                trackInformation.Item.Name,
                                artists,
                                trackInformation.Item.Album.Name,
                                trackInformation.Item.Id);

                            if (Globals.SaveAlbumArtwork)
                            {
                                this.DownloadSpotifyAlbumArtwork(trackInformation.Item.Album);
                            }

                            // Set the last title to the track id as these are unique values that only change when the track changes
                            this.LastTitle = trackInformation.Item.Id;

                            this.snipReset = false;
                    }
                }
            }
        }

        private void DownloadSpotifyAlbumArtwork(SimpleAlbum album)
        {
            string albumId = album.Id;
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
                        Uri imageUrl = new Uri(album.Images[0].Url);

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

        private static Uri SelectAlbumArtworkSizeToDownload(SimpleAlbum album)
        {
            // This assumes that the Spotify image array will always have three results (which in all of my tests it has so far)
            string imageUrl = string.Empty;

            switch (Globals.ArtworkResolution)
            {
                case Globals.AlbumArtworkResolution.Large:
                    imageUrl = album.Images[0].Url;
                    break;

                case Globals.AlbumArtworkResolution.Medium:
                    imageUrl = album.Images[1].Url;
                    break;

                case Globals.AlbumArtworkResolution.Small:
                    imageUrl = album.Images[2].Url;
                    break;

                default:
                    imageUrl = album.Images[2].Url;
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

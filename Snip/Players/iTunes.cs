#region File Information
/*
 * Copyright (C) 2012-2021 David Rudie
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
    using System.Globalization;
    using System.Windows.Forms;
    using iTunesLib;

    internal sealed class Itunes : MediaPlayer
    {
        private iTunesApp ItunesApplication = null;
 
        // This will hold the volume prior to it being muted and restored from it.
        private int muteVolume = 0;

        private delegate void Router(object arg);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        public override void Load()
        {
            try
            {
                this.ItunesApplication = new iTunesApp();

                this.ItunesApplication.OnPlayerPlayEvent += new _IiTunesEvents_OnPlayerPlayEventEventHandler(this.App_OnPlayerPlayEvent);
                this.ItunesApplication.OnPlayerPlayingTrackChangedEvent += new _IiTunesEvents_OnPlayerPlayingTrackChangedEventEventHandler(this.App_OnPlayerPlayingTrackChangedEvent);
                this.ItunesApplication.OnPlayerStopEvent += new _IiTunesEvents_OnPlayerStopEventEventHandler(this.App_OnPlayerStopEvent);
                this.ItunesApplication.OnQuittingEvent += new _IiTunesEvents_OnQuittingEventEventHandler(this.App_OnPlayerQuittingEvent);
            }
            catch (System.Runtime.InteropServices.COMException comException)
            {
                MessageBox.Show(
                        LocalizedMessages.ItunesException + "\n\n" + comException.Message,
                        LocalizedMessages.SnipForm,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }
        }

        public override void Unload()
        {
            base.Unload();

            if (this.ItunesApplication != null)
            {
                this.ItunesApplication.OnPlayerPlayEvent -= this.App_OnPlayerPlayEvent;
                this.ItunesApplication.OnPlayerPlayingTrackChangedEvent -= this.App_OnPlayerPlayingTrackChangedEvent;
                this.ItunesApplication.OnPlayerStopEvent -= this.App_OnPlayerStopEvent;
                this.ItunesApplication.OnQuittingEvent -= this.App_OnPlayerQuittingEvent;

                this.ItunesApplication = null;
            }
        }

        public override void ChangeToNextTrack()
        {
            this.ItunesApplication.NextTrack();
        }

        public override void ChangeToPreviousTrack()
        {
            this.ItunesApplication.PreviousTrack();
        }

        public override void IncreasePlayerVolume()
        {
            this.ItunesApplication.SoundVolume++;
        }

        public override void DecreasePlayerVolume()
        {
            this.ItunesApplication.SoundVolume--;
        }

        public override void MutePlayerAudio()
        {
            if (this.ItunesApplication.SoundVolume > 0)
            {
                this.muteVolume = this.ItunesApplication.SoundVolume;
                this.ItunesApplication.SoundVolume = 0;
            }
            else
            {
                this.ItunesApplication.SoundVolume = this.muteVolume;
            }
        }

        public override void PlayOrPauseTrack()
        {
            this.ItunesApplication.Play();
        }

        public override void PauseTrack()
        {
            this.ItunesApplication.Pause();
        }

        public override void StopTrack()
        {
            this.ItunesApplication.Stop();
        }

        private void App_OnPlayerPlayEvent(object sender)
        {
            IITTrack track = this.ItunesApplication.CurrentTrack;

            if (!string.IsNullOrEmpty(track.Artist) && !string.IsNullOrEmpty(track.Name) && string.IsNullOrEmpty(this.ItunesApplication.CurrentStreamTitle))
            {
                if (Globals.SaveAlbumArtwork)
                {
                    IITArtworkCollection artworkCollection = track.Artwork;

                    if (artworkCollection.Count > 0)
                    {
                        IITArtwork artwork = artworkCollection[1];

                        artwork.SaveArtworkToFile(this.DefaultArtworkFilePath);
                    }
                    else
                    {
                        this.SaveBlankImage();
                    }
                }

                TextHandler.UpdateText(track.Name, track.Artist, track.Album);
            }
            else if (string.IsNullOrEmpty(track.Artist) && !string.IsNullOrEmpty(track.Name) && string.IsNullOrEmpty(this.ItunesApplication.CurrentStreamTitle))
            {
                TextHandler.UpdateText(track.Name);
            }
            else if (!string.IsNullOrEmpty(this.ItunesApplication.CurrentStreamTitle))
            {
                TextHandler.UpdateText(this.ItunesApplication.CurrentStreamTitle);
            }
        }

        private void App_OnPlayerPlayingTrackChangedEvent(object sender)
        {
            IITTrack track = this.ItunesApplication.CurrentTrack;

            if (!string.IsNullOrEmpty(track.Artist) && !string.IsNullOrEmpty(track.Name) && string.IsNullOrEmpty(this.ItunesApplication.CurrentStreamTitle))
            {
                if (Globals.SaveAlbumArtwork)
                {
                    try
                    {
                        IITArtworkCollection artworkCollection = track.Artwork;
                        IITArtwork artwork = artworkCollection[1];

                        artwork.SaveArtworkToFile(this.DefaultArtworkFilePath);
                    }
                    catch
                    {
                        this.SaveBlankImage();
                        throw;
                    }
                }

                TextHandler.UpdateText(track.Name, track.Artist, track.Album);
            }
            else if (!string.IsNullOrEmpty(this.ItunesApplication.CurrentStreamTitle))
            {
                TextHandler.UpdateText(this.ItunesApplication.CurrentStreamTitle);
            }
        }

        private void App_OnPlayerStopEvent(object o)
        {
            if (Globals.SaveAlbumArtwork)
            {
                this.SaveBlankImage();
            }

            TextHandler.UpdateTextAndEmptyFilesMaybe(LocalizedMessages.NoTrackPlaying);
        }

        private void App_OnPlayerQuittingEvent()
        {
            if (Globals.SaveAlbumArtwork)
            {
                this.SaveBlankImage();
            }

            TextHandler.UpdateTextAndEmptyFilesMaybe(
                string.Format(
                    CultureInfo.InvariantCulture,
                    LocalizedMessages.PlayerIsNotRunning,
                    LocalizedMessages.Itunes));
        }
    }
}

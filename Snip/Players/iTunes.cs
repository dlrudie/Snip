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
    using System.Globalization;
    using System.Windows.Forms;
    using iTunesLib;

    internal sealed class iTunes : MediaPlayer
    {
        private iTunesApp iTunesApplication = null;
 
        // This will hold the volume prior to it being muted and restored from it.
        private int muteVolume = 0;

        private delegate void Router(object arg);

        public override void Load()
        {
            try
            {
                this.iTunesApplication = new iTunesApp();

                this.iTunesApplication.OnPlayerPlayEvent += new _IiTunesEvents_OnPlayerPlayEventEventHandler(this.App_OnPlayerPlayEvent);
                this.iTunesApplication.OnPlayerPlayingTrackChangedEvent += new _IiTunesEvents_OnPlayerPlayingTrackChangedEventEventHandler(this.App_OnPlayerPlayingTrackChangedEvent);
                this.iTunesApplication.OnPlayerStopEvent += new _IiTunesEvents_OnPlayerStopEventEventHandler(this.App_OnPlayerStopEvent);
                this.iTunesApplication.OnQuittingEvent += new _IiTunesEvents_OnQuittingEventEventHandler(this.App_OnPlayerQuittingEvent);
            }
            catch (System.Runtime.InteropServices.COMException comException)
            {
                MessageBox.Show(LocalizedMessages.iTunesException + "\n\n" + comException.Message, LocalizedMessages.SnipForm, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public override void Unload()
        {
            base.Unload();

            if (this.iTunesApplication != null)
            {
                this.iTunesApplication.OnPlayerPlayEvent -= this.App_OnPlayerPlayEvent;
                this.iTunesApplication.OnPlayerPlayingTrackChangedEvent -= this.App_OnPlayerPlayingTrackChangedEvent;
                this.iTunesApplication.OnPlayerStopEvent -= this.App_OnPlayerStopEvent;
                this.iTunesApplication.OnQuittingEvent -= this.App_OnPlayerQuittingEvent;

                this.iTunesApplication = null;
            }
        }

        public override void ChangeToNextTrack()
        {
            this.iTunesApplication.NextTrack();
        }

        public override void ChangeToPreviousTrack()
        {
            this.iTunesApplication.PreviousTrack();
        }

        public override void IncreasePlayerVolume()
        {
            this.iTunesApplication.SoundVolume++;
        }

        public override void DecreasePlayerVolume()
        {
            this.iTunesApplication.SoundVolume--;
        }

        public override void MutePlayerAudio()
        {
            if (this.iTunesApplication.SoundVolume > 0)
            {
                this.muteVolume = this.iTunesApplication.SoundVolume;
                this.iTunesApplication.SoundVolume = 0;
            }
            else
            {
                this.iTunesApplication.SoundVolume = this.muteVolume;
            }
        }

        public override void PlayOrPauseTrack()
        {
            this.iTunesApplication.Play();
        }

        public override void PauseTrack()
        {
            this.iTunesApplication.Pause();
        }

        public override void StopTrack()
        {
            this.iTunesApplication.Stop();
        }

        private void App_OnPlayerPlayEvent(object sender)
        {
            IITTrack track = this.iTunesApplication.CurrentTrack;

            if (!string.IsNullOrEmpty(track.Artist) && !string.IsNullOrEmpty(track.Name) && string.IsNullOrEmpty(this.iTunesApplication.CurrentStreamTitle))
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
            else if (string.IsNullOrEmpty(track.Artist) && !string.IsNullOrEmpty(track.Name) && string.IsNullOrEmpty(this.iTunesApplication.CurrentStreamTitle))
            {
                TextHandler.UpdateText(track.Name);
            }
            else if (!string.IsNullOrEmpty(this.iTunesApplication.CurrentStreamTitle))
            {
                TextHandler.UpdateText(this.iTunesApplication.CurrentStreamTitle);
            }
        }

        private void App_OnPlayerPlayingTrackChangedEvent(object sender)
        {
            IITTrack track = this.iTunesApplication.CurrentTrack;

            if (!string.IsNullOrEmpty(track.Artist) && !string.IsNullOrEmpty(track.Name) && string.IsNullOrEmpty(this.iTunesApplication.CurrentStreamTitle))
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
            else if (!string.IsNullOrEmpty(this.iTunesApplication.CurrentStreamTitle))
            {
                TextHandler.UpdateText(this.iTunesApplication.CurrentStreamTitle);
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
                    LocalizedMessages.iTunes));
        }
    }
}

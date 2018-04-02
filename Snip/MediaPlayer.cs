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
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    public class MediaPlayer
    {
        // http://garethrees.org/2007/11/14/pngcrush/
        private readonly byte[] blankImage = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
            0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
            0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
            0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00,
            0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
            0x42, 0x60, 0x82
        };

        private readonly string defaultArtworkFile = @Application.StartupPath + @"\Snip_Artwork.jpg";

        private StringBuilder title = new StringBuilder(256);

        public bool Found { get; set; }

        public bool NotRunning { get; set; }

        public bool SavedBlankImage { get; set; }

        public IntPtr Handle { get; set; }

        public StringBuilder Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.title = value;
            }
        }

        public string LastTitle { get; set; }

        public string DefaultArtworkFilePath
        {
            get
            {
                return this.defaultArtworkFile;
            }
        }

        public virtual void Load()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void Unload()
        {
            this.SaveBlankImage();
            this.Found = false;
            this.Handle = IntPtr.Zero;
            this.NotRunning = true;
            this.LastTitle = string.Empty;
            this.Title.Clear();
        }

        public virtual void ChangeToNextTrack()
        {
        }

        public virtual void ChangeToPreviousTrack()
        {
        }

        public virtual void IncreasePlayerVolume()
        {
        }

        public virtual void DecreasePlayerVolume()
        {
        }

        public virtual void MutePlayerAudio()
        {
        }

        public virtual void PlayOrPauseTrack()
        {
        }

        public virtual void PauseTrack()
        {
        }

        public virtual void StopTrack()
        {
        }

        public void SaveBlankImage()
        {
            try
            {
                File.WriteAllBytes(this.defaultArtworkFile, this.blankImage);
                this.SavedBlankImage = true;
            }
            catch (IOException)
            {
                // File is in use... or something.  We can't write so we'll just bail out and hope no one notices.
            }
        }
    }
}

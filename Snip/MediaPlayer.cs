#region File Information
/*
 * Copyright (C) 2012-2014 David Rudie
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
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    public class MediaPlayer
    {
        private readonly byte[] blankImage = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
            0x89, 0x00, 0x00, 0x00, 0x14, 0x49, 0x44, 0x41,
            0x54, 0x78, 0x5E, 0x15, 0xC0, 0x01, 0x09, 0x00,
            0x00, 0x00, 0x80, 0xA0, 0xFE, 0xAF, 0x0E, 0x8D,
            0x01, 0x00, 0x05, 0x00, 0x01, 0x83, 0xC3, 0xE1,
            0xDD, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E,
            0x44, 0xAE, 0x42, 0x60, 0x82
        };

        private readonly string defaultArtworkFile = @Application.StartupPath + @"\Snip_Artwork.jpg";

        private StringBuilder title = new StringBuilder(256);

        public bool Found { get; set; }

        public bool NotRunning { get; set; }

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
            Image image = ImageFromByteArray(this.blankImage);
            image.Save(this.defaultArtworkFile);
        }

        private static Image ImageFromByteArray(byte[] imageByteArray)
        {
            Image image = null;

            using (MemoryStream memoryStream = new MemoryStream(imageByteArray))
            {
                image = Image.FromStream(memoryStream);
            }

            return image;
        }
    }
}
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
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Forms;
    using Microsoft.Win32;

    public partial class OutputFormat : Form
    {
        private string trackFormat;
        private string separatorFormat;
        private string artistFormat;
        private string albumFormat;

        public OutputFormat()
        {
            this.InitializeComponent();

            this.LoadSettings();

            try
            {
                this.textBoxTrackFormat.Text = this.trackFormat;
                this.textBoxSeparatorFormat.Text = this.separatorFormat;
                this.textBoxArtistFormat.Text = this.artistFormat;
                this.textBoxAlbumFormat.Text = this.albumFormat;
            }
            catch
            {
                this.SetDefaults();
                throw;
            }
        }

        private void ButtonDefaults_Click(object sender, EventArgs e)
        {
            this.SetDefaults();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            this.SaveSettings();

            this.Close();
        }

        private void LoadSettings()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "SOFTWARE\\{0}\\{1}",
                    AssemblyInformation.AssemblyTitle,
                    Assembly.GetExecutingAssembly().GetName().Version.Major));

            if (registryKey != null)
            {
                this.trackFormat = Convert.ToString(registryKey.GetValue("Track Format", Globals.DefaultTrackFormat), CultureInfo.CurrentCulture);

                this.separatorFormat = Convert.ToString(registryKey.GetValue("Separator Format", Globals.DefaultSeparatorFormat), CultureInfo.CurrentCulture);

                this.artistFormat = Convert.ToString(registryKey.GetValue("Artist Format", Globals.DefaultArtistFormat), CultureInfo.CurrentCulture);

                this.albumFormat = Convert.ToString(registryKey.GetValue("Album Format", Globals.DefaultAlbumFormat), CultureInfo.CurrentCulture);

                registryKey.Close();
            }
            else
            {
                this.trackFormat = Globals.DefaultTrackFormat;
                this.separatorFormat = Globals.DefaultSeparatorFormat;
                this.artistFormat = Globals.DefaultArtistFormat;
                this.albumFormat = Globals.DefaultAlbumFormat;
            }
        }

        private void SaveSettings()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "SOFTWARE\\{0}\\{1}",
                    AssemblyInformation.AssemblyTitle,
                    Assembly.GetExecutingAssembly().GetName().Version.Major));

            registryKey.SetValue("Track Format", this.textBoxTrackFormat.Text, RegistryValueKind.String);

            registryKey.SetValue("Separator Format", this.textBoxSeparatorFormat.Text, RegistryValueKind.String);

            registryKey.SetValue("Artist Format", this.textBoxArtistFormat.Text, RegistryValueKind.String);

            registryKey.SetValue("Album Format", this.textBoxAlbumFormat.Text, RegistryValueKind.String);

            registryKey.Close();

            Globals.RewriteUpdatedOutputFormat = true;
        }

        private void SetDefaults()
        {
            this.textBoxTrackFormat.Text = Globals.DefaultTrackFormat;
            this.textBoxSeparatorFormat.Text = Globals.DefaultSeparatorFormat;
            this.textBoxArtistFormat.Text = Globals.DefaultArtistFormat;
            this.textBoxAlbumFormat.Text = Globals.DefaultAlbumFormat;

            Globals.RewriteUpdatedOutputFormat = true;
        }
    }
}

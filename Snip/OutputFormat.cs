#region File Information
//-----------------------------------------------------------------------------
// <copyright file="OutputFormat.cs" company="David Rudie">
//     Copyright (C) 2013 David Rudie
//
//     This program is free software; you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation; either version 3 of the License, or
//     (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program; if not, write to the Free Software
//     Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02111, USA.
// </copyright>
//-----------------------------------------------------------------------------
#endregion

namespace Snip
{
    using System;
    using System.Text;
    using System.Windows.Forms;
    using Microsoft.Win32;

    /// <summary>
    /// This class is for setting the output format of the saved text.
    /// </summary>
    public partial class OutputFormat : Form
    {
        /// <summary>
        /// The name of this program.
        /// </summary>
        private readonly string assemblyTitle = AssemblyInformation.AssemblyTitle;

        /// <summary>
        /// The author of this program.
        /// </summary>
        private readonly string assemblyAuthor = AssemblyInformation.AssemblyAuthor;

        /// <summary>
        /// The version number of this program.
        /// </summary>
        private readonly string assemblyVersion = AssemblyInformation.AssemblyVersion;

        /// <summary>
        /// The default track output format.
        /// </summary>
        private string trackFormat = "“$t”";

        /// <summary>
        /// The default separator output format.
        /// </summary>
        private string separatorFormat = " ― ";

        /// <summary>
        /// The default artist output format.
        /// </summary>
        private string artistFormat = "$a";

        /// <summary>
        /// The default album output format.
        /// </summary>
        private string albumFormat = "$l";

        /// <summary>
        /// Initializes a new instance of the OutputFormat class.
        /// </summary>
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
            }
        }

        /// <summary>
        /// When the Default button is depressed, this method is called and the default values are restored.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void ButtonDefaults_Click(object sender, EventArgs e)
        {
            this.SetDefaults();
        }

        /// <summary>
        /// When the Save button is depressed, this method is called and the current values are saved to the registry.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            this.SaveSettings();

            this.Close();
        }

        /// <summary>
        /// When the Load button is depressed, this method is called and the saved values in the registry are loaded.
        /// </summary>
        private void LoadSettings()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                string.Format(
                    "SOFTWARE\\{0}\\{1}\\{2}",
                    this.assemblyAuthor,
                    this.assemblyTitle,
                    this.assemblyVersion));

            if (registryKey != null)
            {
                this.trackFormat = Convert.ToString(registryKey.GetValue("Track Format", "“$t”"));

                this.separatorFormat = Convert.ToString(registryKey.GetValue("Separator Format", " ― "));

                this.artistFormat = Convert.ToString(registryKey.GetValue("Artist Format", "$a"));

                this.albumFormat = Convert.ToString(registryKey.GetValue("Album Format", "$l"));

                registryKey.Close();
            }
        }

        /// <summary>
        /// Saves current settings to the registry.
        /// </summary>
        private void SaveSettings()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(
                string.Format(
                    "SOFTWARE\\{0}\\{1}\\{2}",
                    this.assemblyAuthor,
                    this.assemblyTitle,
                    this.assemblyVersion));

            registryKey.SetValue("Track Format", this.textBoxTrackFormat.Text, RegistryValueKind.String);

            registryKey.SetValue("Separator Format", this.textBoxSeparatorFormat.Text, RegistryValueKind.String);

            registryKey.SetValue("Artist Format", this.textBoxArtistFormat.Text, RegistryValueKind.String);

            registryKey.SetValue("Album Format", this.textBoxAlbumFormat.Text, RegistryValueKind.String);

            registryKey.Close();
        }

        /// <summary>
        /// Restores the default values.
        /// </summary>
        private void SetDefaults()
        {
            this.trackFormat = "“$t”";
            this.textBoxTrackFormat.Text = this.trackFormat;

            this.separatorFormat = " ― ";
            this.textBoxSeparatorFormat.Text = this.separatorFormat;

            this.artistFormat = "$a";
            this.textBoxArtistFormat.Text = this.artistFormat;

            this.albumFormat = "$l";
            this.textBoxAlbumFormat.Text = this.albumFormat;
        }
    }
}

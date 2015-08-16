#region File Information
/*
 * Copyright (C) 2012-2015 David Rudie
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
    using System.Resources;
    using System.Windows.Forms;
    using Microsoft.Win32;

    public partial class Snip : Form
    {
        #region Fields

        KeyboardHook keyboardHook = new KeyboardHook();

        #endregion

        #region Constructor

        public Snip()
        {
            Globals.ResourceManager = ResourceManager.CreateFileBasedResourceManager("Strings", Application.StartupPath + @"/Resources", null);
            Globals.DefaultTrackFormat = Globals.ResourceManager.GetString("TrackFormat");
            Globals.DefaultSeparatorFormat = " " + Globals.ResourceManager.GetString("SeparatorFormat") + " ";
            Globals.DefaultArtistFormat = Globals.ResourceManager.GetString("ArtistFormat");
            Globals.DefaultAlbumFormat = Globals.ResourceManager.GetString("AlbumFormat");

            this.InitializeComponent();

            this.Load += new EventHandler(this.Snip_Load);
            this.FormClosing += new FormClosingEventHandler(this.Snip_FormClosing);

            // Set the icon of the system tray icon.
            this.notifyIcon.Icon = Properties.Resources.SnipIcon;
            Globals.SnipNotifyIcon = this.notifyIcon;

            // Minimize the main window.
            this.WindowState = FormWindowState.Minimized;

            // Create a blank media player so that the initial call to Unload() won't fuck shit up.
            Globals.CurrentPlayer = new MediaPlayer();

            this.LoadSettings();
            this.timerScanMediaPlayer.Enabled = true;

            // Register global hotkeys
            this.ToggleHotkeys();
        }

        #endregion

        #region Methods

        private void KeyboardHook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.OemOpenBrackets:
                    Globals.CurrentPlayer.ChangeToPreviousTrack();
                    break;

                case Keys.OemCloseBrackets:
                    Globals.CurrentPlayer.ChangeToNextTrack();
                    break;

                case Keys.OemMinus:
                    Globals.CurrentPlayer.DecreasePlayerVolume();
                    break;

                case Keys.Oemplus:
                    Globals.CurrentPlayer.IncreasePlayerVolume();
                    break;

                case Keys.M:
                    Globals.CurrentPlayer.MutePlayerAudio();
                    break;

                case Keys.Enter:
                    Globals.CurrentPlayer.PlayOrPauseTrack();
                    break;

                case Keys.P:
                    Globals.CurrentPlayer.PauseTrack();
                    break;

                case Keys.Back:
                    Globals.CurrentPlayer.StopTrack();
                    break;
            }
        }

        private void Snip_Load(object sender, EventArgs e)
        {
            // Hide the window from ever showing.
            this.Hide();
        }

        private void Snip_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save settings automatically when the form is being closed.
            Settings.Save();
        }

        private void LoadSettings()
        {
            Settings.Load();

            switch (Globals.PlayerSelection)
            {
                case Globals.MediaPlayerSelection.Spotify:
                    this.ToggleSpotify();
                    break;

                case Globals.MediaPlayerSelection.iTunes:
                    this.ToggleiTunes();
                    break;

                case Globals.MediaPlayerSelection.Winamp:
                    this.ToggleWinamp();
                    break;

                case Globals.MediaPlayerSelection.foobar2000:
                    this.Togglefoobar2000();
                    break;

                case Globals.MediaPlayerSelection.VLC:
                    this.ToggleVLC();
                    break;
            }

            this.toolStripMenuItemSaveSeparateFiles.Checked = Globals.SaveSeparateFiles;
            this.toolStripMenuItemSaveAlbumArtwork.Checked = Globals.SaveAlbumArtwork;
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.Checked = Globals.KeepSpotifyAlbumArtwork;

            switch (Globals.ArtworkResolution)
            {
                case Globals.AlbumArtworkResolution.Tiny:
                    this.ToggleArtworkTiny();
                    break;

                case Globals.AlbumArtworkResolution.Small:
                    this.ToggleArtworkSmall();
                    break;

                case Globals.AlbumArtworkResolution.Medium:
                    this.ToggleArtworkMedium();
                    break;

                case Globals.AlbumArtworkResolution.Large:
                    this.ToggleArtworkLarge();
                    break;
            }

            this.toolStripMenuItemSaveHistory.Checked = Globals.SaveHistory;
            this.toolStripMenuItemDisplayTrackPopup.Checked = Globals.DisplayTrackPopup;
            this.toolStripMenuItemEmptyFileIfNoTrackPlaying.Checked = Globals.EmptyFileIfNoTrackPlaying;
            this.toolStripMenuItemEnableHotkeys.Checked = Globals.EnableHotkeys;
        }

        private void ToggleHotkeys()
        {
            if (this.toolStripMenuItemEnableHotkeys.Checked)
            {
                if (this.keyboardHook == null)
                {
                    this.keyboardHook = new KeyboardHook();
                }

                this.keyboardHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(KeyboardHook_KeyPressed);
                this.keyboardHook.RegisterHotkey(ModifierHookKeys.Control | ModifierHookKeys.Alt, Keys.OemOpenBrackets);    // [
                this.keyboardHook.RegisterHotkey(ModifierHookKeys.Control | ModifierHookKeys.Alt, Keys.OemCloseBrackets);   // ]
                this.keyboardHook.RegisterHotkey(ModifierHookKeys.Control | ModifierHookKeys.Alt, Keys.Oemplus);            // +
                this.keyboardHook.RegisterHotkey(ModifierHookKeys.Control | ModifierHookKeys.Alt, Keys.OemMinus);           // -
                this.keyboardHook.RegisterHotkey(ModifierHookKeys.Control | ModifierHookKeys.Alt, Keys.Enter);              // Enter
                this.keyboardHook.RegisterHotkey(ModifierHookKeys.Control | ModifierHookKeys.Alt, Keys.Back);               // Backspace
                this.keyboardHook.RegisterHotkey(ModifierHookKeys.Control | ModifierHookKeys.Alt, Keys.M);                  // M
                this.keyboardHook.RegisterHotkey(ModifierHookKeys.Control | ModifierHookKeys.Alt, Keys.P);                  // P
            }
            else
            {
                if (this.keyboardHook != null)
                {
                    this.keyboardHook.Dispose();
                    this.keyboardHook = null;
                }
            }
        }

        private void PlayerSelectionCheck(object sender, EventArgs e)
        {
            if (sender == this.toolStripMenuItemSpotify)
            {
                this.ToggleSpotify();
            }
            else if (sender == this.toolStripMenuItemItunes)
            {
                this.ToggleiTunes();
            }
            else if (sender == this.toolStripMenuItemWinamp)
            {
                this.ToggleWinamp();
            }
            else if (sender == this.toolStripMenuItemFoobar2000)
            {
                this.Togglefoobar2000();
            }
            else if (sender == this.toolStripMenuItemVlc)
            {
                this.ToggleVLC();
            }
        }

        private void ToggleSpotify()
        {
            this.toolStripMenuItemSpotify.Checked = true;
            this.toolStripMenuItemItunes.Checked = false;
            this.toolStripMenuItemWinamp.Checked = false;
            this.toolStripMenuItemFoobar2000.Checked = false;
            this.toolStripMenuItemVlc.Checked = false;

            Globals.CurrentPlayer.Unload();
            Globals.CurrentPlayer = new Spotify();
            Globals.CurrentPlayer.Load();

            Globals.PlayerSelection = Globals.MediaPlayerSelection.Spotify;
            TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("SwitchedToSpotify"));
        }

        private void ToggleiTunes()
        {
            this.toolStripMenuItemSpotify.Checked = false;
            this.toolStripMenuItemItunes.Checked = true;
            this.toolStripMenuItemWinamp.Checked = false;
            this.toolStripMenuItemFoobar2000.Checked = false;
            this.toolStripMenuItemVlc.Checked = false;

            Globals.CurrentPlayer.Unload();
            Globals.CurrentPlayer = new iTunes();
            Globals.CurrentPlayer.Load();

            Globals.PlayerSelection = Globals.MediaPlayerSelection.iTunes;
            TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("SwitchedToiTunes"));
        }

        private void ToggleWinamp()
        {
            this.toolStripMenuItemSpotify.Checked = false;
            this.toolStripMenuItemItunes.Checked = false;
            this.toolStripMenuItemWinamp.Checked = true;
            this.toolStripMenuItemFoobar2000.Checked = false;
            this.toolStripMenuItemVlc.Checked = false;

            Globals.CurrentPlayer.Unload();
            Globals.CurrentPlayer = new Winamp();
            Globals.CurrentPlayer.Load();

            Globals.PlayerSelection = Globals.MediaPlayerSelection.Winamp;
            TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("SwitchedToWinamp"));
        }

        private void Togglefoobar2000()
        {
            this.toolStripMenuItemSpotify.Checked = false;
            this.toolStripMenuItemItunes.Checked = false;
            this.toolStripMenuItemWinamp.Checked = false;
            this.toolStripMenuItemFoobar2000.Checked = true;
            this.toolStripMenuItemVlc.Checked = false;

            Globals.CurrentPlayer.Unload();
            Globals.CurrentPlayer = new foobar2000();
            Globals.CurrentPlayer.Load();

            Globals.PlayerSelection = Globals.MediaPlayerSelection.foobar2000;
            TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("SwitchedTofoobar2000"));
        }

        private void ToggleVLC()
        {
            this.toolStripMenuItemSpotify.Checked = false;
            this.toolStripMenuItemItunes.Checked = false;
            this.toolStripMenuItemWinamp.Checked = false;
            this.toolStripMenuItemFoobar2000.Checked = false;
            this.toolStripMenuItemVlc.Checked = true;

            Globals.CurrentPlayer.Unload();
            Globals.CurrentPlayer = new VLC();
            Globals.CurrentPlayer.Load();

            Globals.PlayerSelection = Globals.MediaPlayerSelection.VLC;
            TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("SwitchedToVLC"));
        }

        private void ToolStripMenuItemSaveSeparateFiles_Click(object sender, EventArgs e)
        {
            if (this.toolStripMenuItemSaveSeparateFiles.Checked)
            {
                this.toolStripMenuItemSaveSeparateFiles.Checked = false;
            }
            else
            {
                this.toolStripMenuItemSaveSeparateFiles.Checked = true;
            }

            Globals.SaveSeparateFiles = this.toolStripMenuItemSaveSeparateFiles.Checked;
        }

        private void ToolStripMenuItemSaveAlbumArtwork_Click(object sender, EventArgs e)
        {
            if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
            {
                this.toolStripMenuItemSaveAlbumArtwork.Checked = false;
            }
            else
            {
                this.toolStripMenuItemSaveAlbumArtwork.Checked = true;
            }

            Globals.SaveAlbumArtwork = this.toolStripMenuItemSaveAlbumArtwork.Checked;
        }

        private void ToolStripMenuItemKeepSpotifyAlbumArtwork_Click(object sender, EventArgs e)
        {
            if (this.toolStripMenuItemKeepSpotifyAlbumArtwork.Checked)
            {
                this.toolStripMenuItemKeepSpotifyAlbumArtwork.Checked = false;
            }
            else
            {
                this.toolStripMenuItemKeepSpotifyAlbumArtwork.Checked = true;
            }

            Globals.KeepSpotifyAlbumArtwork = this.toolStripMenuItemKeepSpotifyAlbumArtwork.Checked;
        }

        private void ToolStripMenuItemSaveHistory_Click(object sender, EventArgs e)
        {
            if (this.toolStripMenuItemSaveHistory.Checked)
            {
                this.toolStripMenuItemSaveHistory.Checked = false;
            }
            else
            {
                this.toolStripMenuItemSaveHistory.Checked = true;
            }

            Globals.SaveHistory = this.toolStripMenuItemSaveHistory.Checked;
        }

        private void ToolStripMenuItemDisplayTrackPopup_Click(object sender, EventArgs e)
        {
            if (this.toolStripMenuItemDisplayTrackPopup.Checked)
            {
                this.toolStripMenuItemDisplayTrackPopup.Checked = false;
            }
            else
            {
                this.toolStripMenuItemDisplayTrackPopup.Checked = true;
            }

            Globals.DisplayTrackPopup = this.toolStripMenuItemDisplayTrackPopup.Checked;
        }

        private void ToolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void TimerScanMediaPlayer_Tick(object sender, EventArgs e)
        {
            // Make sure this is set before starting the timer.
            //if (Globals.DebuggingIsEnabled)
            //{
                //Debug.MeasureMethod(Globals.CurrentPlayer.Update); // Writes a LOT of data
            //}
            //else
            //{
                Globals.CurrentPlayer.Update();
            //}
        }

        private void ToolStripMenuItemSetFormat_Click(object sender, EventArgs e)
        {
            OutputFormat outputFormat = null;

            try
            {
                outputFormat = new OutputFormat();
                outputFormat.ShowDialog();
            }
            finally
            {
                if (outputFormat != null)
                {
                    outputFormat.Dispose();
                }
            }

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "SOFTWARE\\{0}\\{1}\\{2}",
                    AssemblyInformation.AssemblyAuthor,
                    AssemblyInformation.AssemblyTitle,
                    AssemblyInformation.AssemblyVersion));

            if (registryKey != null)
            {
                Globals.TrackFormat = Convert.ToString(registryKey.GetValue("Track Format", Globals.DefaultTrackFormat), CultureInfo.CurrentCulture);

                Globals.SeparatorFormat = Convert.ToString(registryKey.GetValue("Separator Format", Globals.DefaultSeparatorFormat), CultureInfo.CurrentCulture);

                Globals.ArtistFormat = Convert.ToString(registryKey.GetValue("Artist Format", Globals.DefaultArtistFormat), CultureInfo.CurrentCulture);

                Globals.AlbumFormat = Convert.ToString(registryKey.GetValue("Album Format", Globals.DefaultAlbumFormat), CultureInfo.CurrentCulture);

                registryKey.Close();
            }
        }
        
        private void AlbumArtworkResolutionCheck(object sender, EventArgs e)
        {
            if (sender == this.toolStripMenuItemTiny)
            {
                this.ToggleArtworkTiny();
            }
            else if (sender == this.toolStripMenuItemSmall)
            {
                this.ToggleArtworkSmall();
            }
            else if (sender == this.toolStripMenuItemMedium)
            {
                this.ToggleArtworkMedium();
            }
            else if (sender == this.toolStripMenuItemLarge)
            {
                this.ToggleArtworkLarge();
            }
        }

        private void ToggleArtworkTiny()
        {
            this.toolStripMenuItemTiny.Checked = true;
            this.toolStripMenuItemSmall.Checked = false;
            this.toolStripMenuItemMedium.Checked = false;
            this.toolStripMenuItemLarge.Checked = false;

            Globals.ArtworkResolution = Globals.AlbumArtworkResolution.Tiny;
        }

        private void ToggleArtworkSmall()
        {
            this.toolStripMenuItemTiny.Checked = false;
            this.toolStripMenuItemSmall.Checked = true;
            this.toolStripMenuItemMedium.Checked = false;
            this.toolStripMenuItemLarge.Checked = false;

            Globals.ArtworkResolution = Globals.AlbumArtworkResolution.Small;
        }

        private void ToggleArtworkMedium()
        {
            this.toolStripMenuItemTiny.Checked = false;
            this.toolStripMenuItemSmall.Checked = false;
            this.toolStripMenuItemMedium.Checked = true;
            this.toolStripMenuItemLarge.Checked = false;

            Globals.ArtworkResolution = Globals.AlbumArtworkResolution.Medium;
        }

        private void ToggleArtworkLarge()
        {
            this.toolStripMenuItemTiny.Checked = false;
            this.toolStripMenuItemSmall.Checked = false;
            this.toolStripMenuItemMedium.Checked = false;
            this.toolStripMenuItemLarge.Checked = true;

            Globals.ArtworkResolution = Globals.AlbumArtworkResolution.Large;
        }

        private void ToolStripMenuItemEmptyFileIfNoTrackPlaying_Click(object sender, EventArgs e)
        {
            if (this.toolStripMenuItemEmptyFileIfNoTrackPlaying.Checked)
            {
                this.toolStripMenuItemEmptyFileIfNoTrackPlaying.Checked = false;
            }
            else
            {
                this.toolStripMenuItemEmptyFileIfNoTrackPlaying.Checked = true;
            }

            Globals.EmptyFileIfNoTrackPlaying = this.toolStripMenuItemEmptyFileIfNoTrackPlaying.Checked;
        }

        private void ToolStripMenuItemEnableHotkeys_Click(object sender, EventArgs e)
        {
            if (this.toolStripMenuItemEnableHotkeys.Checked)
            {
                this.toolStripMenuItemEnableHotkeys.Checked = false;
                this.ToggleHotkeys();
            }
            else
            {
                this.toolStripMenuItemEnableHotkeys.Checked = true;
                this.ToggleHotkeys();
            }

            Globals.EnableHotkeys = this.toolStripMenuItemEnableHotkeys.Checked;
        }

        #endregion
    }
}

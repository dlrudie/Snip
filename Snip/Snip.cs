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

/*
 * use Globals.* to set all toolstrip menu items and to load from them as well
 * across all files
 * 
 * MediaPlayer.Load() -- prepares the player for usage
 * MediaPlayer.Update() -- updates the text
 * MediaPlayer.Unload() -- cleans up the player in preparation to switch
 * 
 * issues:
 * saving and loading do not work correctly just yet
 * if other player or resolution selected, it will set the checkmark right but it will also set the default
 * 
 * look at output formatting!
 */

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

        // There are none.

        #endregion

        #region Constructor

        public Snip()
        {
            Globals.ResourceManager = ResourceManager.CreateFileBasedResourceManager("Strings", Application.StartupPath + @"/Resources", null);

            this.InitializeComponent();

            this.Load += new EventHandler(this.Snip_Load);

            // Set the icon of the system tray icon.
            this.notifyIcon.Icon = new Icon(this.Icon, 48, 48);
            Globals.SnipNotifyIcon = this.notifyIcon;

            // Minimize the main window.
            this.WindowState = FormWindowState.Minimized;

            // Create a blank media player so that the initial call to Unload() won't fuck shit up.
            Globals.CurrentPlayer = new MediaPlayer();

            this.LoadSettings();
            this.timerScanMediaPlayer.Enabled = true;
        }

        #endregion

        #region Methods

        private void Snip_Load(object sender, EventArgs e)
        {
            // Hide the window from ever showing.
            this.Hide();
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
        }

        private void ToggleSpotify()
        {
            this.toolStripMenuItemSpotify.Checked = true;
            this.toolStripMenuItemItunes.Checked = false;
            this.toolStripMenuItemWinamp.Checked = false;
            this.toolStripMenuItemFoobar2000.Checked = false;

            Globals.CurrentPlayer.Unload();
            Globals.CurrentPlayer = new Spotify();
            Globals.CurrentPlayer.Load();

            Globals.PlayerSelection = Globals.MediaPlayerSelection.Spotify;
            TextHandler.UpdateTextAndEmptyFile(Globals.ResourceManager.GetString("SwitchedToSpotify"));
        }

        private void ToggleiTunes()
        {
            this.toolStripMenuItemSpotify.Checked = false;
            this.toolStripMenuItemItunes.Checked = true;
            this.toolStripMenuItemWinamp.Checked = false;
            this.toolStripMenuItemFoobar2000.Checked = false;

            Globals.CurrentPlayer.Unload();
            Globals.CurrentPlayer = new iTunes();
            Globals.CurrentPlayer.Load();

            Globals.PlayerSelection = Globals.MediaPlayerSelection.iTunes;
            TextHandler.UpdateTextAndEmptyFile(Globals.ResourceManager.GetString("SwitchedToiTunes"));
        }

        private void ToggleWinamp()
        {
            this.toolStripMenuItemSpotify.Checked = false;
            this.toolStripMenuItemItunes.Checked = false;
            this.toolStripMenuItemWinamp.Checked = true;
            this.toolStripMenuItemFoobar2000.Checked = false;

            Globals.CurrentPlayer.Unload();
            Globals.CurrentPlayer = new Winamp();
            Globals.CurrentPlayer.Load();

            Globals.PlayerSelection = Globals.MediaPlayerSelection.Winamp;
            TextHandler.UpdateTextAndEmptyFile(Globals.ResourceManager.GetString("SwitchedToWinamp"));
        }

        private void Togglefoobar2000()
        {
            this.toolStripMenuItemSpotify.Checked = false;
            this.toolStripMenuItemItunes.Checked = false;
            this.toolStripMenuItemWinamp.Checked = false;
            this.toolStripMenuItemFoobar2000.Checked = true;

            Globals.CurrentPlayer.Unload();
            Globals.CurrentPlayer = new foobar2000();
            Globals.CurrentPlayer.Load();

            Globals.PlayerSelection = Globals.MediaPlayerSelection.foobar2000;
            TextHandler.UpdateTextAndEmptyFile(Globals.ResourceManager.GetString("SwitchedTofoobar2000"));
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

        private void ToolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            Settings.Save();

            Application.Exit();
        }

        private void TimerScanMediaPlayer_Tick(object sender, EventArgs e)
        {
            // Make sure this is set before starting the timer.
            Globals.CurrentPlayer.Update();
        }

        private void TimerHotkey_Tick(object sender, EventArgs e)
        {
            int keyControl = UnsafeNativeMethods.GetAsyncKeyState((int)Keys.ControlKey);
            int keyAlt = UnsafeNativeMethods.GetAsyncKeyState((int)Keys.Menu);

            int keyStateNextTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.NextTrack) & 0x8000;           // S I W
            int keyStatePreviousTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.PreviousTrack) & 0x8000;   // S I W
            int keyStateVolumeUp = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.VolumeUp) & 0x8000;             // S I W
            int keyStateVolumeDown = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.VolumeDown) & 0x8000;         // S I W
            int keyStateMuteTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.MuteTrack) & 0x8000;           // S   W
            int keyStatePlayPauseTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.PlayPauseTrack) & 0x8000; // S I W
            int keyStatePauseTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.PauseTrack) & 0x8000;         //   I
            int keyStateStopTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.StopTrack) & 0x8000;           // S I W

            if (keyControl != 0 && keyAlt != 0)
            {
                if (keyStateNextTrack > 0 && keyStateNextTrack != KeyState.NextTrack)
                {
                    Globals.CurrentPlayer.ChangeToNextTrack();
                }

                if (keyStatePreviousTrack > 0 && keyStatePreviousTrack != KeyState.PreviousTrack)
                {
                    Globals.CurrentPlayer.ChangeToPreviousTrack();
                }

                if (keyStateVolumeUp > 0 && keyStateVolumeUp != KeyState.VolumeUp)
                {
                    Globals.CurrentPlayer.IncreasePlayerVolume();
                }

                if (keyStateVolumeDown > 0 && keyStateVolumeDown != KeyState.VolumeDown)
                {
                    Globals.CurrentPlayer.DecreasePlayerVolume();
                }

                if (keyStateMuteTrack > 0 && keyStateMuteTrack != KeyState.MuteTrack)
                {
                    Globals.CurrentPlayer.MutePlayerAudio();
                }

                if (keyStatePlayPauseTrack > 0 && keyStatePlayPauseTrack != KeyState.PlayPauseTrack)
                {
                    Globals.CurrentPlayer.PlayOrPauseTrack();
                }

                if (keyStatePauseTrack > 0 && keyStatePauseTrack != KeyState.PauseTrack)
                {
                    Globals.CurrentPlayer.PauseTrack();
                }

                if (keyStateStopTrack > 0 && keyStateStopTrack != KeyState.StopTrack)
                {
                    Globals.CurrentPlayer.StopTrack();
                }
            }

            KeyState.NextTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.NextTrack) & 0x8000;
            KeyState.PreviousTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.PreviousTrack) & 0x8000;
            KeyState.VolumeUp = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.VolumeUp) & 0x8000;
            KeyState.VolumeDown = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.VolumeDown) & 0x8000;
            KeyState.MuteTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.MuteTrack) & 0x8000;
            KeyState.PlayPauseTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.PlayPauseTrack) & 0x8000;
            KeyState.PauseTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.PauseTrack) & 0x8000;
            KeyState.StopTrack = UnsafeNativeMethods.GetAsyncKeyState(KeyBinds.StopTrack) & 0x8000;
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
                Globals.TrackFormat = Convert.ToString(registryKey.GetValue("Track Format", "“$t”"), CultureInfo.CurrentCulture);

                Globals.SeparatorFormat = Convert.ToString(registryKey.GetValue("Separator Format", " ― "), CultureInfo.CurrentCulture);

                Globals.ArtistFormat = Convert.ToString(registryKey.GetValue("Artist Format", "$a"), CultureInfo.CurrentCulture);

                Globals.AlbumFormat = Convert.ToString(registryKey.GetValue("Album Format", "$l"), CultureInfo.CurrentCulture);

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

        #endregion
    }
}

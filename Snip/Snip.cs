#region File Information
//-----------------------------------------------------------------------------
// <copyright file="Snip.cs" company="David Rudie">
//     Copyright (C) 2012, 2013 David Rudie
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
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Windows.Forms;
    using iTunesLib;
    using Microsoft.Win32;
    using SimpleJson;

    /// <summary>
    /// This class is used for reading the playing track title and artist from either iTunes or Spotify.
    /// </summary>
    public partial class Snip : Form
    {
        #region Fields

        /// <summary>
        /// This is a alpha transparent 1x1 PNG image.
        /// </summary>
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
        /// Used to store key states as they are depressed or released.
        /// </summary>
        private KeyState keyState;

        /// <summary>
        /// Contains the default key binds used for hotkeys.
        /// </summary>
        private KeyBinds keyBinds;

        /// <summary>
        /// This will be used to do everything and anything with iTunes.
        /// </summary>
        private iTunesApp itunes = null;

        /// <summary>
        /// This can be used to check if a new iTunesApp() has been created and set up.
        /// </summary>
        private bool itunesSetup = false;

        /// <summary>
        /// Used to store information about Spotify.
        /// </summary>
        private MediaPlayer spotifyApp;

        /// <summary>
        /// Used to store information about Winamp;
        /// </summary>
        private MediaPlayer winampApp;

        /// <summary>
        /// This will hold the last title that was checked.  If it is different than before we can update the text file and system tray text.
        /// </summary>
        private string lastTitle = string.Empty;

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

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Snip class.
        /// </summary>
        public Snip()
        {
            this.InitializeComponent();
            this.Load += new EventHandler(this.Snip_Load);

            // Set the icon of the system tray icon.
            this.notifyIcon.Icon = Properties.Resources.Snip;

            // Minimize the main window.
            this.WindowState = FormWindowState.Minimized;

            // Load settings from the registry.
            this.LoadSettings();

            this.keyState = new KeyState();

            this.keyBinds = new KeyBinds();

            this.spotifyApp = new MediaPlayer();
            this.winampApp = new MediaPlayer();
        }

        #endregion

        #region Delegates

        /// <summary>
        /// This router will help in handling iTunes events.
        /// </summary>
        /// <param name="arg">The object arguments.</param>
        private delegate void Router(object arg);

        #endregion

        #region Enumerations

        /// <summary>
        /// This enum contains the supported players.
        /// </summary>
        private enum PlayerSelection : int
        {
            /// <summary>
            /// Spotify identification.
            /// </summary>
            Spotify = 0,

            /// <summary>
            /// iTunes identification.
            /// </summary>
            iTunes = 1,

            /// <summary>
            /// Winamp identification.
            /// </summary>
            Winamp = 2
        }

        /// <summary>
        /// This enum contains the commands to send to Spotify or Winamp.
        /// </summary>
        private enum MediaCommands : long
        {
            /// <summary>
            /// Plays or pauses the current track.
            /// </summary>
            PlayPauseTrack = 0xE0000,

            /// <summary>
            /// Mutes the audio.
            /// </summary>
            MuteTrack = 0x80000,

            /// <summary>
            /// Lowers the volume.
            /// </summary>
            VolumeDown = 0x90000,

            /// <summary>
            /// Raises the volume.
            /// </summary>
            VolumeUp = 0xA0000,

            /// <summary>
            /// Stops the current track.
            /// </summary>
            StopTrack = 0xD0000,

            /// <summary>
            /// Switches to the previous track.
            /// </summary>
            PreviousTrack = 0xC0000,

            /// <summary>
            /// Switches to the next track.
            /// </summary>
            NextTrack = 0xB0000
        }

        #endregion

        #region Methods

        /// <summary>
        /// Anything under here will be called as soon as the form loads.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Snip_Load(object sender, EventArgs e)
        {
            // Hide the window from ever showing.
            this.Hide();
        }

        /// <summary>
        /// AUTOBOTS...ROLL OUT!
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ToolStripMenuItemSpotify_Click(object sender, EventArgs e)
        {
            this.UpdatePlayer(PlayerSelection.Spotify);
        }

        /// <summary>
        /// Tells the program that we only want to look for iTunes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void ToolStripMenuItemItunes_Click(object sender, EventArgs e)
        {
            this.UpdatePlayer(PlayerSelection.iTunes);
        }

        /// <summary>
        /// Tells the program that we only want to look for Winamp.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void ToolStripMenuItemWinamp_Click(object sender, EventArgs e)
        {
            this.UpdatePlayer(PlayerSelection.Winamp);
        }

        private void UpdatePlayer(PlayerSelection playerSelection)
        {
            switch (playerSelection)
            {
                case PlayerSelection.Spotify:
                    this.toolStripMenuItemSpotify.Checked = true;
                    this.toolStripMenuItemItunes.Checked = false;
                    this.toolStripMenuItemWinamp.Checked = false;

                    this.UpdateTextAndEmptyFile("Switched to Spotify");

                    this.itunes = null;
                    this.itunesSetup = false;

                    break;

                case PlayerSelection.iTunes:
                    this.toolStripMenuItemSpotify.Checked = false;
                    this.toolStripMenuItemItunes.Checked = true;
                    this.toolStripMenuItemWinamp.Checked = false;

                    this.UpdateTextAndEmptyFile("Switched to iTunes");

                    if (this.itunes == null && !this.itunesSetup)
                    {
                        this.SetUpItunes();
                    }

                    break;

                case PlayerSelection.Winamp:
                    this.toolStripMenuItemSpotify.Checked = false;
                    this.toolStripMenuItemItunes.Checked = false;
                    this.toolStripMenuItemWinamp.Checked = true;

                    this.UpdateTextAndEmptyFile("Switched to Winamp");

                    this.itunes = null;
                    this.itunesSetup = false;

                    break;

                default:
                    this.toolStripMenuItemSpotify.Checked = true;
                    this.toolStripMenuItemItunes.Checked = false;
                    this.toolStripMenuItemWinamp.Checked = false;

                    this.UpdateTextAndEmptyFile("Switched to Spotify");

                    this.itunes = null;
                    this.itunesSetup = false;

                    break;
            }
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
        }

        private void ToolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            // Save settings to the registry.
            this.SaveSettings();

            // Exit the application.
            Application.Exit();
        }

        /// <summary>
        /// This method will be used to scan for Spotify/Winamp title updates.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TimerScanTitle_Tick(object sender, EventArgs e)
        {
            if (this.toolStripMenuItemSpotify.Checked)
            {
                if (!this.spotifyApp.Found)
                {
                    this.spotifyApp.Handle = UnsafeNativeMethods.FindWindow("SpotifyMainWindow", null);

                    this.spotifyApp.Found = true;
                    this.spotifyApp.NotRunning = false;
                }
                else
                {
                    // Make sure the process is still valid.
                    if (this.spotifyApp.Handle != IntPtr.Zero && this.spotifyApp.Handle != null)
                    {
                        int windowTextLength = UnsafeNativeMethods.GetWindowText(this.spotifyApp.Handle, this.spotifyApp.Title, this.spotifyApp.Title.Capacity);

                        string spotifyTitle = this.spotifyApp.Title.ToString();

                        this.spotifyApp.Title.Clear();

                        // If the window title length is 0 then the process handle is not valid.
                        if (windowTextLength > 0)
                        {
                            // Only update the system tray text and text file text if the title changes.
                            if (spotifyTitle != this.lastTitle)
                            {
                                if (spotifyTitle == "Spotify")
                                {
                                    if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                                    {
                                        this.SaveBlankImage();
                                    }

                                    this.UpdateTextAndEmptyFile("No track playing");
                                }
                                else
                                {
                                    // Spotify window titles look like "Spotify - Artist - Song Title".
                                    string windowTitleFull = spotifyTitle.Replace("Spotify - ", string.Empty);
                                    string[] windowTitle = windowTitleFull.Split('–');

                                    string artist = windowTitle[0].Trim();
                                    string songTitle = windowTitle[1].Trim();

                                    if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                                    {
                                        string trackId = string.Empty;

                                        try
                                        {
                                            var json = new WebClient().DownloadString(
                                                string.Format(
                                                    "http://ws.spotify.com/search/1/track.json?q={0}+{1}",
                                                    HttpUtility.UrlEncode(artist),
                                                    HttpUtility.UrlEncode(songTitle)));

                                            dynamic jsonSummary = SimpleJson.DeserializeObject(json);

                                            if (jsonSummary != null)
                                            {
                                                jsonSummary = SimpleJson.DeserializeObject(jsonSummary["tracks"][0].ToString());

                                                if (jsonSummary != null)
                                                {
                                                    trackId = jsonSummary.href.ToString();
                                                    trackId = trackId.Substring(trackId.LastIndexOf(':') + 1);

                                                    using (WebClient webClient = new WebClient())
                                                    {
                                                        json = webClient.DownloadString(string.Format("https://embed.spotify.com/oembed/?url=spotify:track:{0}", trackId));

                                                        jsonSummary = SimpleJson.DeserializeObject(json);

                                                        string imageUrl = jsonSummary.thumbnail_url.ToString().Replace("cover", "640");

                                                        webClient.DownloadFile(new Uri(imageUrl), @"Snip_Artwork.jpg");
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            this.SaveBlankImage();
                                        }
                                    }

                                    this.UpdateText(songTitle, artist);
                                }

                                this.lastTitle = spotifyTitle;
                            }
                        }
                        else
                        {
                            if (!this.spotifyApp.NotRunning)
                            {
                                if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                                {
                                    this.SaveBlankImage();
                                }

                                this.UpdateTextAndEmptyFile("Spotify is not currently running");
                                this.spotifyApp.Found = false;
                                this.spotifyApp.NotRunning = true;
                            }
                        }
                    }
                    else
                    {
                        if (!this.spotifyApp.NotRunning)
                        {
                            if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                            {
                                this.SaveBlankImage();
                            }

                            this.UpdateTextAndEmptyFile("Spotify is not currently running");
                            this.spotifyApp.Found = false;
                            this.spotifyApp.NotRunning = true;
                        }
                    }
                }
            }
            else if (this.toolStripMenuItemWinamp.Checked)
            {
                if (!this.winampApp.Found)
                {
                    this.winampApp.Handle = UnsafeNativeMethods.FindWindow("Winamp v1.x", null);

                    this.winampApp.Found = true;
                    this.winampApp.NotRunning = false;
                }
                else
                {
                    // Make sure the process is still valid.
                    if (this.winampApp.Handle != IntPtr.Zero && this.winampApp.Handle != null)
                    {
                        int windowTextLength = UnsafeNativeMethods.GetWindowText(this.winampApp.Handle, this.winampApp.Title, this.winampApp.Title.Capacity);

                        string winampTitle = this.winampApp.Title.ToString();

                        this.winampApp.Title.Clear();

                        // If the window title length is 0 then the process handle is not valid.
                        if (windowTextLength > 0)
                        {
                            // Only update the system tray text and text file text if the title changes.
                            if (winampTitle != this.lastTitle)
                            {
                                if (winampTitle.Contains("- Winamp [Stopped]") || winampTitle.Contains("- Winamp [Paused]"))
                                {
                                    this.UpdateTextAndEmptyFile("No track playing");
                                }
                                else
                                {
                                    // Winamp window titles look like "#. Artist - Track - Winamp".
                                    // Require that the user use ATF and replace the format with something like:
                                    // %artist% – %title%
                                    string windowTitleFull = winampTitle.Replace("- Winamp", string.Empty);
                                    string[] windowTitle = windowTitleFull.Split('–');

                                    // Album artwork not supported by Winamp
                                    if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                                    {
                                        this.SaveBlankImage();
                                    }

                                    if (windowTitle.Length > 1)
                                    {
                                        string artist = windowTitle[0].Trim();
                                        string songTitle = windowTitle[1].Trim();

                                        this.UpdateText(songTitle, artist);
                                    }
                                    else
                                    {
                                        this.UpdateText(windowTitle[0].Trim());
                                    }
                                }

                                this.lastTitle = winampTitle;
                            }
                        }
                        else
                        {
                            if (!this.winampApp.NotRunning)
                            {
                                if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                                {
                                    this.SaveBlankImage();
                                }

                                this.UpdateTextAndEmptyFile("Winamp is not currently running");
                                this.winampApp.Found = false;
                                this.winampApp.NotRunning = true;
                            }
                        }
                    }
                    else
                    {
                        if (!this.winampApp.NotRunning)
                        {
                            if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                            {
                                this.SaveBlankImage();
                            }

                            this.UpdateTextAndEmptyFile("Winamp is not currently running");
                            this.winampApp.Found = false;
                            this.winampApp.NotRunning = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This will capture keys pressed and send a command to the Spotify application.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TimerHotkey_Tick(object sender, EventArgs e)
        {
            int keyControl = UnsafeNativeMethods.GetAsyncKeyState((int)Keys.ControlKey);
            int keyAlt = UnsafeNativeMethods.GetAsyncKeyState((int)Keys.Menu);

            int keyStateNextTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.NextTrack) & 0x8000;           //// S I W
            int keyStatePreviousTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.PreviousTrack) & 0x8000;   //// S I W
            int keyStateVolumeUp = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.VolumeUp) & 0x8000;             //// S I W
            int keyStateVolumeDown = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.VolumeDown) & 0x8000;         //// S I W
            int keyStateMuteTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.MuteTrack) & 0x8000;           //// S   W
            int keyStatePlayPauseTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.PlayPauseTrack) & 0x8000; //// S I W
            int keyStatePauseTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.PauseTrack) & 0x8000;         ////   I
            int keyStateStopTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.StopTrack) & 0x8000;           //// S I W

            if (keyControl != 0 && keyAlt != 0 && keyStateNextTrack > 0 && keyStateNextTrack != this.keyState.NextTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.NextTrack));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.itunes.NextTrack();
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.NextTrack));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStatePreviousTrack > 0 && keyStatePreviousTrack != this.keyState.PreviousTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.PreviousTrack));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.itunes.PreviousTrack();
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.PreviousTrack));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStateVolumeUp > 0 && keyStateVolumeUp != this.keyState.VolumeUp)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeUp));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.itunes.SoundVolume++;
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeUp));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStateVolumeDown > 0 && keyStateVolumeDown != this.keyState.VolumeDown)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeDown));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.itunes.SoundVolume--;
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeDown));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStateMuteTrack > 0 && keyStateMuteTrack != this.keyState.MuteTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.MuteTrack));
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.MuteTrack));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStatePlayPauseTrack > 0 && keyStatePlayPauseTrack != this.keyState.PlayPauseTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.PlayPauseTrack));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.itunes.Play();
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.PlayPauseTrack));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStatePauseTrack > 0 && keyStatePauseTrack != this.keyState.PauseTrack)
            {
                if (this.toolStripMenuItemItunes.Checked)
                {
                    this.itunes.Pause();
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStateStopTrack > 0 && keyStateStopTrack != this.keyState.StopTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.StopTrack));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.itunes.Stop();
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampApp.Handle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.StopTrack));
                }
            }

            this.keyState.NextTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.NextTrack) & 0x8000;
            this.keyState.PreviousTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.PreviousTrack) & 0x8000;
            this.keyState.VolumeUp = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.VolumeUp) & 0x8000;
            this.keyState.VolumeDown = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.VolumeDown) & 0x8000;
            this.keyState.MuteTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.MuteTrack) & 0x8000;
            this.keyState.PlayPauseTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.PlayPauseTrack) & 0x8000;
            this.keyState.PauseTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.PauseTrack) & 0x8000;
            this.keyState.StopTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyBinds.StopTrack) & 0x8000;
        }

        /// <summary>
        /// This will do garbage collection, trim the process working size, and then compact the process's heap.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TimerMinimizeMemory_Tick(object sender, EventArgs e)
        {
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            UnsafeNativeMethods.SetProcessWorkingSetSize(
                System.Diagnostics.Process.GetCurrentProcess().Handle,
                (UIntPtr)0xFFFFFFFF,
                (UIntPtr)0xFFFFFFFF);

            IntPtr heap = UnsafeNativeMethods.GetProcessHeap();

            if (UnsafeNativeMethods.HeapLock(heap))
            {
                try
                {
                    if (UnsafeNativeMethods.HeapCompact(heap, 0) == 0)
                    {
                        // Error condition ignored.
                    }
                }
                finally
                {
                    UnsafeNativeMethods.HeapUnlock(heap);
                }
            }
        }

        private void SetUpItunes()
        {
            if (!this.itunesSetup)
            {
                this.itunes = new iTunesApp();

                // This will call App_OnPlayerPlayEvent when a track is played for the first time.
                this.itunes.OnPlayerPlayEvent += new _IiTunesEvents_OnPlayerPlayEventEventHandler(delegate(object o)
                {
                    this.Invoke(new Router(this.App_OnPlayerPlayEvent), o);
                });

                // This will call App_OnPlayerPlayingTrackChangedEvent when a playing track changes to another track.
                this.itunes.OnPlayerPlayingTrackChangedEvent += new _IiTunesEvents_OnPlayerPlayingTrackChangedEventEventHandler(delegate(object o)
                {
                    this.Invoke(new Router(this.App_OnPlayerPlayingTrackChangedEvent), o);
                });

                // This will call App_OnPlayerStopEvent when a playing track is stopped.
                this.itunes.OnPlayerStopEvent += new _IiTunesEvents_OnPlayerStopEventEventHandler(delegate(object o)
                {
                    this.Invoke(new Router(this.App_OnPlayerStopEvent), o);
                });

                // This will call App_OnPlayerQuittingEvent when iTunes is terminated.
                this.itunes.OnQuittingEvent += new _IiTunesEvents_OnQuittingEventEventHandler(this.App_OnPlayerQuittingEvent);

                this.itunesSetup = true;
            }
        }

        /// <summary>
        /// This event is called when a track is played for the first time.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void App_OnPlayerPlayEvent(object sender)
        {
            IITTrack track = this.itunes.CurrentTrack;

            if (!string.IsNullOrEmpty(track.Artist) && !string.IsNullOrEmpty(track.Name) && string.IsNullOrEmpty(this.itunes.CurrentStreamTitle))
            {
                if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                {
                    try
                    {
                        IITArtworkCollection artworkCollection = track.Artwork;
                        IITArtwork artwork = artworkCollection[1];

                        artwork.SaveArtworkToFile(@Application.StartupPath + @"\Snip_Artwork.jpg");
                    }
                    catch
                    {
                        this.SaveBlankImage();
                    }
                }

                this.UpdateText(track.Name, track.Artist, track.Album);
            }
            else if (!string.IsNullOrEmpty(this.itunes.CurrentStreamTitle))
            {
                this.UpdateText(this.itunes.CurrentStreamTitle);
            }
        }

        /// <summary>
        /// This event is called when an already playing track is changed to another track.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void App_OnPlayerPlayingTrackChangedEvent(object sender)
        {
            IITTrack track = this.itunes.CurrentTrack;

            if (!string.IsNullOrEmpty(track.Artist) && !string.IsNullOrEmpty(track.Name) && string.IsNullOrEmpty(this.itunes.CurrentStreamTitle))
            {
                if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                {
                    try
                    {
                        IITArtworkCollection artworkCollection = track.Artwork;
                        IITArtwork artwork = artworkCollection[1];

                        artwork.SaveArtworkToFile(@Application.StartupPath + @"\Snip_Artwork.jpg");
                    }
                    catch
                    {
                        this.SaveBlankImage();
                    }
                }

                this.UpdateText(track.Name, track.Artist, track.Album);
            }
            else if (!string.IsNullOrEmpty(this.itunes.CurrentStreamTitle))
            {
                this.UpdateText(this.itunes.CurrentStreamTitle);
            }
        }

        /// <summary>
        /// This event is called when a playing track is stopped.
        /// </summary>
        /// <param name="o">The sender.</param>
        private void App_OnPlayerStopEvent(object o)
        {
            if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
            {
                this.SaveBlankImage();
            }

            this.UpdateTextAndEmptyFile("No track playing");
        }

        /// <summary>
        /// This event is called when iTunes is terminated.
        /// </summary>
        private void App_OnPlayerQuittingEvent()
        {
            if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
            {
                this.SaveBlankImage();
            }

            this.UpdateTextAndEmptyFile("iTunes is not currently running");
            //// Application.Exit();
        }

        /// <summary>
        /// This will set the notify icon text correctly if it's over 64 characters long, and truncate it if it's over 128.
        /// </summary>
        /// <param name="notifyIcon">The notify icon we're going to set the text for.</param>
        /// <param name="text">The text to set on the notify icon.</param>
        private void SetNotifyIconText(NotifyIcon notifyIcon, string text)
        {
            int maxLength = 127; // 128 max length minus 1

            if (text.Length >= maxLength)
            {
                int nextSpace = text.LastIndexOf(' ', maxLength);

                text = string.Format("{0} ...", text.Substring(0, (nextSpace > 0) ? nextSpace : maxLength).Trim());
            }

            Type t = typeof(NotifyIcon);

            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;

            t.GetField("text", hidden).SetValue(notifyIcon, text);

            if ((bool)t.GetField("added", hidden).GetValue(notifyIcon))
            {
                t.GetMethod("UpdateIcon", hidden).Invoke(notifyIcon, new object[] { true });
            }
        }

        private void UpdateTextAndEmptyFile(string text)
        {
            this.SetNotifyIconText(this.notifyIcon, text);

            File.WriteAllText(@Application.StartupPath + @"\Snip.txt", string.Empty);
        }

        /// <summary>
        /// This method will update the notify icon text and write text to the snip.txt file within the application's directory.
        /// </summary>
        /// <param name="text">The text to update the notify icon and text file with.</param>
        private void UpdateText(string text)
        {
            // Set the text that appears on the notify icon.
            this.SetNotifyIconText(this.notifyIcon, text);

            // Write the song title and artist to a text file.
            File.WriteAllText(@Application.StartupPath + @"\Snip.txt", text);
        }

        private void UpdateText(string title, string artist)
        {
            this.UpdateText(title, artist, string.Empty);
        }

        private void UpdateText(string title, string artist, string album)
        {
            string output = this.trackFormat + this.separatorFormat + this.artistFormat;

            output = output.Replace("$t", title);
            output = output.Replace("$a", artist);

            if (!string.IsNullOrEmpty(album))
            {
                output = output.Replace("$l", album);
            }

            // Set the text that appears on the notify icon.
            this.SetNotifyIconText(this.notifyIcon, output);

            // Write the song title and artist to a text file.
            File.WriteAllText(@Application.StartupPath + @"\Snip.txt", output);

            // Check if we want to save artist and track to separate files.
            if (this.toolStripMenuItemSaveSeparateFiles.Checked)
            {
                File.WriteAllText(@Application.StartupPath + @"\Snip_Artist.txt", this.artistFormat.Replace("$a", artist));
                File.WriteAllText(@Application.StartupPath + @"\Snip_Track.txt", this.trackFormat.Replace("$t", title));

                if (!string.IsNullOrEmpty(album))
                {
                    File.WriteAllText(@Application.StartupPath + @"\Snip_Album.txt", this.albumFormat.Replace("$l", album));
                }
            }

            // If saving track history is enabled, append that information to a separate file.
            if (this.toolStripMenuItemSaveHistory.Checked)
            {
                File.AppendAllText(@Application.StartupPath + @"\Snip_History.txt", output + Environment.NewLine);
            }
        }

        private void ToolStripMenuItemSetFormat_Click(object sender, EventArgs e)
        {
            OutputFormat outputFormat = new OutputFormat();
            outputFormat.ShowDialog();

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
                PlayerSelection playerSelection = PlayerSelection.Spotify;

                try
                {
                    playerSelection = (PlayerSelection)registryKey.GetValue("Player", PlayerSelection.Spotify);
                }
                catch
                {
                    playerSelection = PlayerSelection.Spotify;
                }

                switch (playerSelection)
                {
                    case PlayerSelection.Spotify:
                        this.toolStripMenuItemSpotify.Checked = true;
                        this.toolStripMenuItemItunes.Checked = false;
                        this.toolStripMenuItemWinamp.Checked = false;

                        break;

                    case PlayerSelection.iTunes:
                        this.toolStripMenuItemSpotify.Checked = false;
                        this.toolStripMenuItemItunes.Checked = true;
                        this.toolStripMenuItemWinamp.Checked = false;

                        this.SetUpItunes();

                        break;

                    case PlayerSelection.Winamp:
                        this.toolStripMenuItemSpotify.Checked = false;
                        this.toolStripMenuItemItunes.Checked = false;
                        this.toolStripMenuItemWinamp.Checked = true;

                        break;

                    default:
                        this.toolStripMenuItemSpotify.Checked = true;
                        this.toolStripMenuItemItunes.Checked = false;
                        this.toolStripMenuItemWinamp.Checked = false;

                        break;
                }

                bool saveSeparateFilesChecked = Convert.ToBoolean(registryKey.GetValue("Save Separate Files", false));

                if (saveSeparateFilesChecked)
                {
                    this.toolStripMenuItemSaveSeparateFiles.Checked = true;
                }
                else
                {
                    this.toolStripMenuItemSaveSeparateFiles.Checked = false;
                }

                bool saveAlbumArtworkChecked = Convert.ToBoolean(registryKey.GetValue("Save Album Artwork", false));

                if (saveAlbumArtworkChecked)
                {
                    this.toolStripMenuItemSaveAlbumArtwork.Checked = true;
                }
                else
                {
                    this.toolStripMenuItemSaveAlbumArtwork.Checked = false;
                }

                bool saveHistoryChecked = Convert.ToBoolean(registryKey.GetValue("Save History", false));

                if (saveHistoryChecked)
                {
                    this.toolStripMenuItemSaveHistory.Checked = true;
                }
                else
                {
                    this.toolStripMenuItemSaveHistory.Checked = false;
                }

                this.trackFormat = Convert.ToString(registryKey.GetValue("Track Format", "“$t”"));

                this.separatorFormat = Convert.ToString(registryKey.GetValue("Separator Format", " ― "));

                this.artistFormat = Convert.ToString(registryKey.GetValue("Artist Format", "$a"));

                this.albumFormat = Convert.ToString(registryKey.GetValue("Album Format", "$l"));

                registryKey.Close();
            }
        }

        private void SaveSettings()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(
                string.Format(
                    "SOFTWARE\\{0}\\{1}\\{2}",
                    this.assemblyAuthor,
                    this.assemblyTitle,
                    this.assemblyVersion));

            if (this.toolStripMenuItemSpotify.Checked)
            {
                registryKey.SetValue("Player", (int)PlayerSelection.Spotify);
            }
            else if (this.toolStripMenuItemItunes.Checked)
            {
                registryKey.SetValue("Player", (int)PlayerSelection.iTunes);
            }
            else if (this.toolStripMenuItemWinamp.Checked)
            {
                registryKey.SetValue("Player", (int)PlayerSelection.Winamp);
            }

            if (this.toolStripMenuItemSaveSeparateFiles.Checked)
            {
                registryKey.SetValue("Save Separate Files", "true");
            }
            else
            {
                registryKey.SetValue("Save Separate Files", "false");
            }

            if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
            {
                registryKey.SetValue("Save Album Artwork", "true");
            }
            else
            {
                registryKey.SetValue("Save Album Artwork", "false");
            }

            if (this.toolStripMenuItemSaveHistory.Checked)
            {
                registryKey.SetValue("Save History", "true");
            }
            else
            {
                registryKey.SetValue("Save History", "false");
            }

            registryKey.Close();
        }
        
        /// <summary>
        /// Converts a byte array of an image into an Image.
        /// </summary>
        /// <param name="imageByteArray">The byte array containing the image.</param>
        /// <returns>An Image created from a byte array.</returns>
        private Image ImageFromByteArray(byte[] imageByteArray)
        {
            MemoryStream memoryStream = new MemoryStream(imageByteArray);
            Image image = Image.FromStream(memoryStream);
            return image;
        }

        private void SaveBlankImage()
        {
            Image image = this.ImageFromByteArray(this.blankImage);
            image.Save(@Application.StartupPath + @"\Snip_Artwork.jpg");
        }

        #endregion

        /// <summary>
        /// This structure will hold the last key state of a hotkey.
        /// </summary>
        public struct KeyState
        {
            public int NextTrack;
            public int PreviousTrack;
            public int VolumeUp;
            public int VolumeDown;
            public int PlayPauseTrack;
            public int StopTrack;
            public int MuteTrack;
            public int PauseTrack;
        }

        /// <summary>
        /// Holds key binds for the hotkeys.
        /// </summary>
        public class KeyBinds
        {
            private int nextTrack = (int)Keys.OemCloseBrackets;      // Default: ]
            private int previousTrack = (int)Keys.OemOpenBrackets;   // Default: [
            private int volumeUp = (int)Keys.Oemplus;                // Default: +
            private int volumeDown = (int)Keys.OemMinus;             // Default: -
            private int playPauseTrack = (int)Keys.Enter;            // Default: Enter
            private int stopTrack = (int)Keys.Back;                  // Default: Backspace
            private int muteTrack = (int)Keys.M;                     // Default: M
            private int pauseTrack = (int)Keys.P;                    // Default: P

            /// <summary>
            /// Gets the key bind used for switching to the next track.
            /// </summary>
            public int NextTrack
            {
                get
                {
                    return this.nextTrack;
                }
            }

            /// <summary>
            /// Gets the keybind used for switching to the previous track.
            /// </summary>
            public int PreviousTrack
            {
                get
                {
                    return this.previousTrack;
                }
            }

            /// <summary>
            /// Gets the key bind used for raising the volume.
            /// </summary>
            public int VolumeUp
            {
                get
                {
                    return this.volumeUp;
                }
            }

            /// <summary>
            /// Gets the key bind used for lowering the volume.
            /// </summary>
            public int VolumeDown
            {
                get
                {
                    return this.volumeDown;
                }
            }

            /// <summary>
            /// Gets the key bind used to play or pause the track.
            /// </summary>
            public int PlayPauseTrack
            {
                get
                {
                    return this.playPauseTrack;
                }
            }

            /// <summary>
            /// Gets the key used to stop playing the currently playing track.
            /// </summary>
            public int StopTrack
            {
                get
                {
                    return this.stopTrack;
                }
            }

            /// <summary>
            /// Gets the key bind used to mute the currently playing track.
            /// </summary>
            public int MuteTrack
            {
                get
                {
                    return this.muteTrack;
                }
            }

            /// <summary>
            /// Gets the key used to pause playback of the currently playing track.
            /// </summary>
            public int PauseTrack
            {
                get
                {
                    return this.pauseTrack;
                }
            }
        }
        
        /// <summary>
        /// This class is meant t... I'm sleepy.
        /// </summary>
        public class MediaPlayer
        {
            /// <summary>
            /// Whether the program was even found.
            /// </summary>
            private bool found = false;

            /// <summary>
            /// Whether the program is running or not.
            /// </summary>
            private bool notRunning = true;

            /// <summary>
            /// The handle of the main window.
            /// </summary>
            private IntPtr handle = IntPtr.Zero;

            /// <summary>
            /// Used for the title of the main window handle.
            /// </summary>
            private StringBuilder title = new StringBuilder(256);

            /// <summary>
            /// Gets or sets a value indicating whether the media player was found in memory.
            /// </summary>
            public bool Found
            {
                get
                {
                    return this.found;
                }

                set
                {
                    this.found = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the media player is an actively running process.
            /// </summary>
            public bool NotRunning
            {
                get
                {
                    return this.notRunning;
                }

                set
                {
                    this.notRunning = value;
                }
            }

            /// <summary>
            /// Gets or sets the pointer to the handle of the media player.
            /// </summary>
            public IntPtr Handle
            {
                get
                {
                    return this.handle;
                }

                set
                {
                    this.handle = value;
                }
            }

            /// <summary>
            /// Gets or sets the window title of the media player.
            /// </summary>
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
        }
    }
}

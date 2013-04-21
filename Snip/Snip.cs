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
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Windows.Forms;
    using iTunesLib;
    using Microsoft.Win32;

    /// <summary>
    /// This class is used for reading the playing track title and artist from either iTunes or Spotify.
    /// </summary>
    public partial class Snip : Form
    {
        #region Fields

        private const string AuthorName = "David Rudie";
        private const string ApplicationName = "Snip";
        private const string ApplicationVersion = "2.5.0";

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
        /// This key will be used to switch to the next track.  Default: ]
        /// </summary>
        private int keyNextTrack = (int)Keys.OemCloseBrackets;

        /// <summary>
        /// This will hold the last key state.
        /// </summary>
        private int lastKeyStateNextTrack = 0;

        /// <summary>
        /// This key will be used to switch to the previous track.  Default: [
        /// </summary>
        private int keyPreviousTrack = (int)Keys.OemOpenBrackets;

        /// <summary>
        /// This will hold the last key state.
        /// </summary>
        private int lastKeyStatePreviousTrack = 0;

        /// <summary>
        /// This key will be used to raise the volume.  Default: +
        /// </summary>
        private int keyVolumeUp = (int)Keys.Oemplus;

        /// <summary>
        /// This will hold the last key state.
        /// </summary>
        private int lastKeyStateVolumeUp = 0;

        /// <summary>
        /// This key will be used to lower the volume.  Default: -
        /// </summary>
        private int keyVolumeDown = (int)Keys.OemMinus;

        /// <summary>
        /// This will hold the last key state.
        /// </summary>
        private int lastKeyStateVolumeDown = 0;

        /// <summary>
        /// This key will be used to play and pause the track.  Default: Enter
        /// </summary>
        private int keyPlayPauseTrack = (int)Keys.Enter;

        /// <summary>
        /// This will hold the last key state.
        /// </summary>
        private int lastKeyStatePlayPauseTrack = 0;

        /// <summary>
        /// This key will be used to stop the track.  Default: Backspace
        /// </summary>
        private int keyStopTrack = (int)Keys.Back;

        /// <summary>
        /// This will hold the last key state.
        /// </summary>
        private int lastKeyStateStopTrack = 0;

        /// <summary>
        /// This key will be used to mute the track.  Default: M
        /// </summary>
        private int keyMuteTrack = (int)Keys.M;

        /// <summary>
        /// This will hold the last key state.
        /// </summary>
        private int lastKeyStateMuteTrack = 0;

        /// <summary>
        /// This key will be used to play the track.  Default: Enter
        /// </summary>
        private int keyPlayTrack = (int)Keys.Enter;

        /// <summary>
        /// This will hold the last key state.
        /// </summary>
        private int lastKeyStatePlayTrack = 0;

        /// <summary>
        /// This key will be used to pause the track.  Default: P
        /// </summary>
        private int keyPauseTrack = (int)Keys.P;

        /// <summary>
        /// This will hold the last key state.
        /// </summary>
        private int lastKeyStatePauseTrack = 0;

        /// <summary>
        /// This will be used to do everything and anything with iTunes.
        /// </summary>
        private iTunesApp iTunes = null;

        /// <summary>
        /// This can be used to check if a new iTunesApp() has been created and set up.
        /// </summary>
        private bool iTunesSetup = false;

        /// <summary>
        /// We will continuously scan for the Spotify process and set this to true if found so no more checks are needed.
        /// </summary>
        private bool spotifyFound = false;

        /// <summary>
        /// Used to limit the amount of text writing if Spotify is not running.
        /// </summary>
        private bool spotifyNotRunning = true;

        /// <summary>
        /// Once we have found Spotify we can set the process here.
        /// </summary>
        private IntPtr spotifyHandle = IntPtr.Zero;

        /// <summary>
        /// This will hold the window title of Spotify.
        /// </summary>
        private StringBuilder spotifyTitle = new StringBuilder(256);

        /// <summary>
        /// We will continuously scan for the Winamp process and set this to true if found so no more checks are needed.
        /// </summary>
        private bool winampFound = false;

        /// <summary>
        /// Used to limit the amount of text writing if Winamp is not running.
        /// </summary>
        private bool winampNotRunning = true;

        /// <summary>
        /// Once we have found Winamp we can set the process here.
        /// </summary>
        private IntPtr winampHandle = IntPtr.Zero;

        /// <summary>
        /// This will hold the window title of Winamp.
        /// </summary>
        private StringBuilder winampTitle = new StringBuilder(256);

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

        private void ToolStripMenuItemSpotify_Click(object sender, EventArgs e)
        {
            this.UpdatePlayer(PlayerSelection.Spotify);
        }

        private void ToolStripMenuItemItunes_Click(object sender, EventArgs e)
        {
            this.UpdatePlayer(PlayerSelection.iTunes);
        }

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

                    this.UpdateText("Switched to Spotify");

                    this.iTunes = null;
                    this.iTunesSetup = false;

                    break;

                case PlayerSelection.iTunes:
                    this.toolStripMenuItemSpotify.Checked = false;
                    this.toolStripMenuItemItunes.Checked = true;
                    this.toolStripMenuItemWinamp.Checked = false;

                    this.UpdateText("Switched to iTunes");

                    if (this.iTunes == null && !this.iTunesSetup)
                    {
                        this.SetUpItunes();
                    }

                    break;

                case PlayerSelection.Winamp:
                    this.toolStripMenuItemSpotify.Checked = false;
                    this.toolStripMenuItemItunes.Checked = false;
                    this.toolStripMenuItemWinamp.Checked = true;

                    this.UpdateText("Switched to Winamp");

                    this.iTunes = null;
                    this.iTunesSetup = false;

                    break;

                default:
                    this.toolStripMenuItemSpotify.Checked = true;
                    this.toolStripMenuItemItunes.Checked = false;
                    this.toolStripMenuItemWinamp.Checked = false;

                    this.UpdateText("Switched to Spotify");

                    this.iTunes = null;
                    this.iTunesSetup = false;

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
                if (!this.spotifyFound)
                {
                    this.spotifyHandle = UnsafeNativeMethods.FindWindow("SpotifyMainWindow", null);

                    this.spotifyFound = true;
                    this.spotifyNotRunning = false;
                }
                else
                {
                    // Make sure the process is still valid.
                    if (this.spotifyHandle != IntPtr.Zero && this.spotifyHandle != null)
                    {
                        int windowTextLength = UnsafeNativeMethods.GetWindowText(this.spotifyHandle, this.spotifyTitle, this.spotifyTitle.Capacity);

                        string spotifyTitle = this.spotifyTitle.ToString();

                        this.spotifyTitle.Clear();

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

                                    this.UpdateText("No track playing");
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
                                        string searchString = string.Format("\"name\":\"{0}\",\"uri\":\"spotify:track:", songTitle);

                                        int addressResult = this.FindInMemory(this.spotifyHandle, searchString);

                                        if (addressResult > 0)
                                        {
                                            addressResult += searchString.Length;

                                            int trackIdSize = 0x16;

                                            byte[] byteBuffer = new byte[trackIdSize];
                                            string trackId = string.Empty;

                                            int processId = 0;
                                            UnsafeNativeMethods.GetWindowThreadProcessId(this.spotifyHandle, out processId);
                                            IntPtr processHandle = UnsafeNativeMethods.OpenProcess(0x10, false, processId);

                                            if (UnsafeNativeMethods.ReadProcessMemory(processHandle, (IntPtr)addressResult, byteBuffer, trackIdSize, IntPtr.Zero))
                                            {
                                                trackId = System.Text.Encoding.ASCII.GetString(byteBuffer);

                                                using (WebClient webClient = new WebClient())
                                                {
                                                    string html = webClient.DownloadString(string.Format("http://open.spotify.com/track/{0}", trackId));

                                                    Regex regex = new Regex("img src=\"(.*)\" border=\"0\" alt=\".*\" id=\"big-cover\"", RegexOptions.Compiled);
                                                    Match match = regex.Match(html);

                                                    try
                                                    {
                                                        webClient.DownloadFile(new Uri(match.Groups[1].Value), @"Snip_Artwork.jpg");
                                                    }
                                                    catch
                                                    {
                                                        this.SaveBlankImage();
                                                    }
                                                }

                                                UnsafeNativeMethods.CloseHandle(processHandle);
                                            }
                                            else
                                            {
                                                UnsafeNativeMethods.CloseHandle(processHandle);
                                                throw new System.ComponentModel.Win32Exception();
                                            }
                                        }
                                        else
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
                            if (!this.spotifyNotRunning)
                            {
                                if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                                {
                                    this.SaveBlankImage();
                                }

                                this.UpdateText("Spotify is not currently running");
                                this.spotifyFound = false;
                                this.spotifyNotRunning = true;
                            }
                        }
                    }
                    else
                    {
                        if (!this.spotifyNotRunning)
                        {
                            if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                            {
                                this.SaveBlankImage();
                            }

                            this.UpdateText("Spotify is not currently running");
                            this.spotifyFound = false;
                            this.spotifyNotRunning = true;
                        }
                    }
                }
            }
            else if (this.toolStripMenuItemWinamp.Checked)
            {
                if (!this.winampFound)
                {
                    this.winampHandle = UnsafeNativeMethods.FindWindow("Winamp v1.x", null);

                    this.winampFound = true;
                    this.winampNotRunning = false;
                }
                else
                {
                    // Make sure the process is still valid.
                    if (this.winampHandle != IntPtr.Zero && this.winampHandle != null)
                    {
                        int windowTextLength = UnsafeNativeMethods.GetWindowText(this.winampHandle, this.winampTitle, this.winampTitle.Capacity);

                        string winampTitle = this.winampTitle.ToString();

                        this.winampTitle.Clear();

                        // If the window title length is 0 then the process handle is not valid.
                        if (windowTextLength > 0)
                        {
                            // Only update the system tray text and text file text if the title changes.
                            if (winampTitle != this.lastTitle)
                            {
                                if (winampTitle.Contains("- Winamp [Stopped]") || winampTitle.Contains("- Winamp [Paused]"))
                                {
                                    this.UpdateText("No track playing");
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
                            if (!this.winampNotRunning)
                            {
                                if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                                {
                                    this.SaveBlankImage();
                                }

                                this.UpdateText("Winamp is not currently running");
                                this.winampFound = false;
                                this.winampNotRunning = true;
                            }
                        }
                    }
                    else
                    {
                        if (!this.winampNotRunning)
                        {
                            if (this.toolStripMenuItemSaveAlbumArtwork.Checked)
                            {
                                this.SaveBlankImage();
                            }

                            this.UpdateText("Winamp is not currently running");
                            this.winampFound = false;
                            this.winampNotRunning = true;
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

            int keyStateNextTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyNextTrack) & 0x8000;           //// S I W
            int keyStatePreviousTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyPreviousTrack) & 0x8000;   //// S I W
            int keyStateVolumeUp = UnsafeNativeMethods.GetAsyncKeyState(this.keyVolumeUp) & 0x8000;             //// S I W
            int keyStateVolumeDown = UnsafeNativeMethods.GetAsyncKeyState(this.keyVolumeDown) & 0x8000;         //// S I W
            int keyStateMuteTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyMuteTrack) & 0x8000;           //// S   W
            int keyStatePlayPauseTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyPlayPauseTrack) & 0x8000; //// S I W
            int keyStatePauseTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyPauseTrack) & 0x8000;         ////   I
            int keyStateStopTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyStopTrack) & 0x8000;           //// S I W

            if (keyControl != 0 && keyAlt != 0 && keyStateNextTrack > 0 && keyStateNextTrack != this.lastKeyStateNextTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.NextTrack));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.iTunes.NextTrack();
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.NextTrack));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStatePreviousTrack > 0 && keyStatePreviousTrack != this.lastKeyStatePreviousTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.PreviousTrack));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.iTunes.PreviousTrack();
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.PreviousTrack));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStateVolumeUp > 0 && keyStateVolumeUp != this.lastKeyStateVolumeUp)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeUp));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.iTunes.SoundVolume++;
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeUp));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStateVolumeDown > 0 && keyStateVolumeDown != this.lastKeyStateVolumeDown)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeDown));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.iTunes.SoundVolume--;
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.VolumeDown));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStateMuteTrack > 0 && keyStateMuteTrack != this.lastKeyStateMuteTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.MuteTrack));
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.MuteTrack));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStatePlayPauseTrack > 0 && keyStatePlayPauseTrack != this.lastKeyStatePlayTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.PlayPauseTrack));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.iTunes.Play();
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.PlayPauseTrack));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStatePauseTrack > 0 && keyStatePauseTrack != this.lastKeyStatePauseTrack)
            {
                if (this.toolStripMenuItemItunes.Checked)
                {
                    this.iTunes.Pause();
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStateStopTrack > 0 && keyStateStopTrack != this.lastKeyStateStopTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.StopTrack));
                }
                else if (this.toolStripMenuItemItunes.Checked)
                {
                    this.iTunes.Stop();
                }
                else if (this.toolStripMenuItemWinamp.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.winampHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)MediaCommands.StopTrack));
                }
            }

            this.lastKeyStateNextTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyNextTrack) & 0x8000;
            this.lastKeyStatePreviousTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyPreviousTrack) & 0x8000;
            this.lastKeyStateVolumeUp = UnsafeNativeMethods.GetAsyncKeyState(this.keyVolumeUp) & 0x8000;
            this.lastKeyStateVolumeDown = UnsafeNativeMethods.GetAsyncKeyState(this.keyVolumeDown) & 0x8000;
            this.lastKeyStateMuteTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyMuteTrack) & 0x8000;
            this.lastKeyStatePlayPauseTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyPlayTrack) & 0x8000;
            this.lastKeyStatePauseTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyPauseTrack) & 0x8000;
            this.lastKeyStateStopTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyStopTrack) & 0x8000;
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
            if (!this.iTunesSetup)
            {
                this.iTunes = new iTunesApp();

                // This will call App_OnPlayerPlayEvent when a track is played for the first time.
                this.iTunes.OnPlayerPlayEvent += new _IiTunesEvents_OnPlayerPlayEventEventHandler(delegate(object o)
                {
                    this.Invoke(new Router(this.App_OnPlayerPlayEvent), o);
                });

                // This will call App_OnPlayerPlayingTrackChangedEvent when a playing track changes to another track.
                this.iTunes.OnPlayerPlayingTrackChangedEvent += new _IiTunesEvents_OnPlayerPlayingTrackChangedEventEventHandler(delegate(object o)
                {
                    this.Invoke(new Router(this.App_OnPlayerPlayingTrackChangedEvent), o);
                });

                // This will call App_OnPlayerStopEvent when a playing track is stopped.
                this.iTunes.OnPlayerStopEvent += new _IiTunesEvents_OnPlayerStopEventEventHandler(delegate(object o)
                {
                    this.Invoke(new Router(this.App_OnPlayerStopEvent), o);
                });

                // This will call App_OnPlayerQuittingEvent when iTunes is terminated.
                this.iTunes.OnQuittingEvent += new _IiTunesEvents_OnQuittingEventEventHandler(this.App_OnPlayerQuittingEvent);

                this.iTunesSetup = true;
            }
        }

        /// <summary>
        /// This event is called when a track is played for the first time.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void App_OnPlayerPlayEvent(object sender)
        {
            IITTrack track = this.iTunes.CurrentTrack;

            if (!string.IsNullOrEmpty(track.Artist) && !string.IsNullOrEmpty(track.Name) && string.IsNullOrEmpty(this.iTunes.CurrentStreamTitle))
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
            else if (!string.IsNullOrEmpty(this.iTunes.CurrentStreamTitle))
            {
                this.UpdateText(this.iTunes.CurrentStreamTitle);
            }
        }

        /// <summary>
        /// This event is called when an already playing track is changed to another track.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void App_OnPlayerPlayingTrackChangedEvent(object sender)
        {
            IITTrack track = this.iTunes.CurrentTrack;

            if (!string.IsNullOrEmpty(track.Artist) && !string.IsNullOrEmpty(track.Name) && string.IsNullOrEmpty(this.iTunes.CurrentStreamTitle))
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
            else if (!string.IsNullOrEmpty(this.iTunes.CurrentStreamTitle))
            {
                this.UpdateText(this.iTunes.CurrentStreamTitle);
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

            this.UpdateText("No track playing");
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

            this.UpdateText("iTunes is not currently running");
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
                    AuthorName,
                    ApplicationName,
                    ApplicationVersion));

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
                    AuthorName,
                    ApplicationName,
                    ApplicationVersion));

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
                    AuthorName,
                    ApplicationName,
                    ApplicationVersion));

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
        /// Finds a byte array within the memory of a process.
        /// </summary>
        /// <param name="processHandle">The process handle of the process you're going to search within.</param>
        /// <param name="searchString">The string to search for.</param>
        /// <returns>The address in memory where the match was found.</returns>
        private int FindInMemory(IntPtr processHandle, string searchString)
        {
            int processId = 0;

            UnsafeNativeMethods.GetWindowThreadProcessId(processHandle, out processId);

            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(processId);

            if (process != null)
            {
                int foundAddress = 0;

                int readSize = 1024 * 64;

                byte[] search = System.Text.Encoding.ASCII.GetBytes(searchString);

                for (int j = 0x20000000; j < 0x30000000; j += readSize)
                {
                    ManagedWinapi.ProcessMemoryChunk memoryChunk = new ManagedWinapi.ProcessMemoryChunk(process, (IntPtr)j, readSize + search.Length);

                    byte[] chunk = memoryChunk.Read();

                    for (int k = 0; k < chunk.Length - search.Length; k++)
                    {
                        bool foundOffset = true;

                        for (int l = 0; l < search.Length; l++)
                        {
                            if (chunk[k + l] != search[l])
                            {
                                foundOffset = false;

                                break;
                            }
                        }

                        if (foundOffset)
                        {
                            foundAddress = k + j;

                            break;
                        }
                    }

                    if (foundAddress != 0)
                    {
                        break;
                    }
                }

                return foundAddress;
            }
            else
            {
                return 0;
            }
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
    }

    #region UnsafeNativeMethods

    /// <summary>
    /// This class holds unsafe native methods.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern short GetAsyncKeyState(
            [In] int keyInt);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(
            [In] string className,
            [In] string windowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(
            [In] IntPtr windowHandle,
            [Out] StringBuilder windowText,
            [In] int maxCount);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetWindowThreadProcessId(
            [In] IntPtr windowHandle,
            [Out] out int processId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(
            [In] IntPtr windowHandle,
            [In] uint message,
            [In] IntPtr wParam,
            [In] IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(
            [In] IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenProcess(
            [In] uint desiredAccess,
            [In] bool inheritHandle,
            [In] int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReadProcessMemory(
            [In] IntPtr process,
            [In] IntPtr baseAddress,
            [Out] byte[] buffer,
            [In] int size,
            [Out] IntPtr bytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetProcessWorkingSetSize(
            [In] IntPtr process,
            [In] UIntPtr minimumWorkingSetSize,
            [In] UIntPtr maximumWorkingSetSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcessHeap();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool HeapLock(
            [In] IntPtr heap);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint HeapCompact(
            [In] IntPtr heap,
            [In] uint flags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool HeapUnlock(
            [In] IntPtr heap);

        /// <summary>
        /// Contains a list of window messages to be used with SendMessage().
        /// </summary>
        internal class WindowMessage
        {
            /// <summary>
            /// Sent when the user selects a command item from a menu, when a control sends a notification message to its parent window, or when an accelerator keystroke is translated.
            /// </summary>
            internal const uint WM_COMMAND = 0x111;

            /// <summary>
            /// Notifies a window that the user generated an application command event, for example, by clicking an application command button using the mouse or typing an application command key on the keyboard.
            /// </summary>
            internal const uint WM_APPCOMMAND = 0x319;

            /// <summary>
            /// Used to define private messages for use by private window classes, usually of the form WM_USER+x, where x is an integer value.
            /// </summary>
            internal const uint WM_USER = 0x400;
        }
    }

    #endregion
}

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

        private readonly string company = "David Rudie";
        private readonly string application = "Snip";
        private readonly string version = "2.0";

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
        /// Used to limit the amount of text writing if spotify is not running.
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
        private string separatorFormat = "―";

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
            /// WinAmp identification.
            /// </summary>
            WinAmp = 2
        }

        /// <summary>
        /// This enum contains the commands to send to Spotify.
        /// </summary>
        private enum SpotifyCommand : long
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

        /// <summary>
        /// This enum contains the commands to send to Winamp.
        /// </summary>
        private enum WinampCommand : long
        {
            /// <summary>
            /// Switches to the previous track.
            /// </summary>
            PreviousTrack = 40044,

            /// <summary>
            /// Plays the current track.
            /// </summary>
            PlayTrack = 40045,

            /// <summary>
            /// Pauses the current track.
            /// </summary>
            PauseTrack = 40046,

            /// <summary>
            /// Stops the current track.
            /// </summary>
            StopTrack = 40047,

            /// <summary>
            /// Switches to the next track.
            /// </summary>
            NextTrack = 40048,

            /// <summary>
            /// Raises the volume.
            /// </summary>
            VolumeUp = 40058,

            /// <summary>
            /// Lowers the volume.
            /// </summary>
            VolumeDown = 40059,
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
            this.UpdatePlayer(PlayerSelection.WinAmp);
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

                case PlayerSelection.WinAmp:
                    this.toolStripMenuItemSpotify.Checked = false;
                    this.toolStripMenuItemItunes.Checked = false;
                    this.toolStripMenuItemWinamp.Checked = true;

                    this.UpdateText("Switched to WinAmp");

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
        /// This method will be used to scan for Spotify title updates.
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
                                        File.Copy(@Application.StartupPath + @"\Snip_Blank.jpg", @Application.StartupPath + @"\Snip_Artwork.jpg", true);
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
                                                    string html = webClient.DownloadString(string.Format("https://embed.spotify.com/?uri=spotify:track:{0}", trackId));

                                                    string dataCa = "data-ca=\"";
                                                    int imageUrlStart = html.IndexOf(dataCa);
                                                    string imageUrl = html.Substring(imageUrlStart + dataCa.Length, 81);

                                                    webClient.DownloadFile(new Uri(imageUrl), @"Snip_Artwork.jpg");
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
                                            File.Copy(@Application.StartupPath + @"\Snip_Blank.jpg", @Application.StartupPath + @"\Snip_Artwork.jpg", true);
                                        }
                                    }

                                    string outputFormat = this.trackFormat + " " + this.separatorFormat + " " + this.artistFormat;

                                    outputFormat = outputFormat.Replace("$t", songTitle);
                                    outputFormat = outputFormat.Replace("$a", artist);

                                    this.UpdateText(outputFormat);
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
                                    File.Copy(@Application.StartupPath + @"\Snip_Blank.jpg", @Application.StartupPath + @"\Snip_Artwork.jpg", true);
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
                                File.Copy(@Application.StartupPath + @"\Snip_Blank.jpg", @Application.StartupPath + @"\Snip_Artwork.jpg", true);
                            }

                            this.UpdateText("Spotify is not currently running");
                            this.spotifyFound = false;
                            this.spotifyNotRunning = true;
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

            int keyStateNextTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyNextTrack) & 0x8000;           // S/I
            int keyStatePreviousTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyPreviousTrack) & 0x8000;   // S/I
            int keyStateVolumeUp = UnsafeNativeMethods.GetAsyncKeyState(this.keyVolumeUp) & 0x8000;             // S
            int keyStateVolumeDown = UnsafeNativeMethods.GetAsyncKeyState(this.keyVolumeDown) & 0x8000;         // S
            int keyStateMuteTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyMuteTrack) & 0x8000;           // S
            int keyStatePlayPauseTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyPlayPauseTrack) & 0x8000; // S/I
            int keyStatePauseTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyPauseTrack) & 0x8000;         // I
            int keyStateStopTrack = UnsafeNativeMethods.GetAsyncKeyState(this.keyStopTrack) & 0x8000;           // S/I

            if (keyControl != 0 && keyAlt != 0 && keyStateNextTrack > 0 && keyStateNextTrack != this.lastKeyStateNextTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)SpotifyCommand.NextTrack));
                }
                else
                {
                    this.iTunes.NextTrack();
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStatePreviousTrack > 0 && keyStatePreviousTrack != this.lastKeyStatePreviousTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)SpotifyCommand.PreviousTrack));
                }
                else
                {
                    this.iTunes.PreviousTrack();
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStateVolumeUp > 0 && keyStateVolumeUp != this.lastKeyStateVolumeUp)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)SpotifyCommand.VolumeUp));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStateVolumeDown > 0 && keyStateVolumeDown != this.lastKeyStateVolumeDown)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)SpotifyCommand.VolumeDown));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStateMuteTrack > 0 && keyStateMuteTrack != this.lastKeyStateMuteTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)SpotifyCommand.MuteTrack));
                }
            }

            if (keyControl != 0 && keyAlt != 0 && keyStatePlayPauseTrack > 0 && keyStatePlayPauseTrack != this.lastKeyStatePlayTrack)
            {
                if (this.toolStripMenuItemSpotify.Checked)
                {
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)SpotifyCommand.PlayPauseTrack));
                }
                else
                {
                    this.iTunes.Play();
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
                    UnsafeNativeMethods.SendMessage(this.spotifyHandle, UnsafeNativeMethods.WindowMessage.WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)SpotifyCommand.StopTrack));
                }
                else
                {
                    this.iTunes.Stop();
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
                        File.Copy(@Application.StartupPath + @"\Snip_Blank.jpg", @Application.StartupPath + @"\Snip_Artwork.jpg", true);
                    }
                }

                string outputFormat = this.trackFormat + " " + this.separatorFormat + " " + this.artistFormat;

                outputFormat = outputFormat.Replace("$t", track.Name);
                outputFormat = outputFormat.Replace("$a", track.Artist);

                this.UpdateText(outputFormat);
            }
            else if (!string.IsNullOrEmpty(this.iTunes.CurrentStreamTitle))
            {
                this.UpdateText(this.iTunes.CurrentStreamTitle);
            }

            /*
            MessageBox.Show(string.Format(
                "0: {0}\n" +
                "1: {1}\n" +
                "2: {2}\n" +
                "3: {3}\n" +
                "4: {4}\n" +
                "5: {5}\n" +
                "6: {6}\n" +
                "7: {7}\n" +
                "8: {8}\n" +
                "9: {9}\n" +
                "10: {10}\n" +
                "11: {11}\n" +
                "12: {12}\n" +
                "13: {13}\n" +
                "14: {14}\n" +
                "15: {15}\n" +
                "16: {16}\n" +
                "17: {17}\n" +
                "18: {18}\n" +
                "19: {19}\n" +
                "20: {20}\n" +
                "21: {21}\n" +
                "22: {22}\n" +
                "23: {23}\n" +
                "24: {24}\n" +
                "25: {25}\n" +
                "26: {26}\n" +
                "27: {27}\n" +
                "28: {28}\n" +
                "29: {29}\n" +
                "30: {30}\n" +
                "31: {31}\n" +
                "32: {32}\n" +
                "33: {33}\n" +
                "34: {34}\n" +
                "35: {35}\n" +
                "36: {36}\n" +
                "37: {37}\n" +
                "38: {38}\n\n" +
                "39: {39}",
                track.Album, // 0
                track.Artist, // 1
                track.Artwork, // 2
                track.BitRate, // 3
                track.BPM, // 4
                track.Comment, // 5
                track.Compilation, // 6
                track.Composer, // 7
                track.DateAdded, // 8
                track.DiscCount, // 9
                track.DiscNumber, // 10
                track.Duration, // 11
                track.Enabled, // 12
                track.EQ, // 13
                track.Finish, // 14
                track.Genre, // 15
                track.Grouping, // 16
                track.Index, // 17
                track.Kind, // 18
                track.KindAsString, // 19
                track.ModificationDate, // 20
                track.Name, // 21
                track.PlayedCount, // 22
                track.PlayedDate, // 23
                track.Playlist, // 24
                track.playlistID, // 25
                track.PlayOrderIndex, // 26
                track.Rating, // 27
                track.SampleRate, // 28
                track.Size, // 29
                track.sourceID, // 30
                track.Start, // 31
                track.Time, // 32
                track.TrackCount, // 33
                track.TrackDatabaseID, // 34
                track.trackID, // 35
                track.TrackNumber, // 36
                track.VolumeAdjustment, // 37
                track.Year, // 38
                iTunes.CurrentStreamTitle)); // 39
             */
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
                        File.Copy(@Application.StartupPath + @"\Snip_Blank.jpg", @Application.StartupPath + @"\Snip_Artwork.jpg", true);
                    }
                }

                string outputFormat = this.trackFormat + " " + this.separatorFormat + " " + this.artistFormat;

                outputFormat = outputFormat.Replace("$t", track.Name);
                outputFormat = outputFormat.Replace("$a", track.Artist);

                this.UpdateText(outputFormat);
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
                File.Copy(@Application.StartupPath + @"\Snip_Blank.jpg", @Application.StartupPath + @"\Snip_Artwork.jpg", true);
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
                File.Copy(@Application.StartupPath + @"\Snip_Blank.jpg", @Application.StartupPath + @"\Snip_Artwork.jpg", true);
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
            //// TODO:
            //// Make this take multiple arguments for track, separator, and artist.

            // Set the text that appears on the notify icon.
            this.SetNotifyIconText(this.notifyIcon, text);

            // Write the song title and artist to a text file.
            File.WriteAllText(@Application.StartupPath + @"\Snip.txt", text);

            // Check if we want to save artist and track to separate files.
            if (this.toolStripMenuItemSaveSeparateFiles.Checked
                    && text != "iTunes is not currently running"
                    && text != "Spotify is not currently running"
                    && text != "No track playing"
                    && text != "Switched to iTunes"
                    && text != "Switched to Spotify")
            {
                string[] songTitleAndArtist = text.Split(new string[] { this.separatorFormat }, StringSplitOptions.None);

                string songTitle = songTitleAndArtist[0];
                string artist = songTitleAndArtist[1];

                File.WriteAllText(@Application.StartupPath + @"\Snip_Artist.txt", artist);
                File.WriteAllText(@Application.StartupPath + @"\Snip_Track.txt", songTitle);
            }

            // If saving track history is enabled, append that information to a separate file.
            if (this.toolStripMenuItemSaveHistory.Checked)
            {
                if (text != "iTunes is not currently running"
                    && text != "Spotify is not currently running"
                    && text != "No track playing"
                    && text != "Switched to iTunes"
                    && text != "Switched to Spotify")
                {
                    File.AppendAllText(@Application.StartupPath + @"\Snip_History.txt", text + Environment.NewLine);
                }
            }
        }

        private void ToolStripMenuItemSetFormat_Click(object sender, EventArgs e)
        {
            OutputFormat outputFormat = new OutputFormat();
            outputFormat.ShowDialog();

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                string.Format(
                    "SOFTWARE\\{0}\\{1}\\{2}",
                    this.company,
                    this.application,
                    this.version));

            if (registryKey != null)
            {
                this.trackFormat = Convert.ToString(registryKey.GetValue("Track Format", "“$t”"));

                this.separatorFormat = Convert.ToString(registryKey.GetValue("Separator Format", "―"));

                this.artistFormat = Convert.ToString(registryKey.GetValue("Artist Format", "$a"));

                registryKey.Close();
            }
        }

        private void LoadSettings()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                string.Format(
                    "SOFTWARE\\{0}\\{1}\\{2}",
                    this.company,
                    this.application,
                    this.version));

            if (registryKey != null)
            {
                // Since this application supports only iTunes or Spotify we can
                // use a single value and check whether it's true or false.
                bool spotifyChecked = Convert.ToBoolean(registryKey.GetValue("Spotify", true));

                if (spotifyChecked)
                {
                    this.toolStripMenuItemSpotify.Checked = true;
                    this.toolStripMenuItemItunes.Checked = false;
                }
                else
                {
                    this.toolStripMenuItemSpotify.Checked = false;
                    this.toolStripMenuItemItunes.Checked = true;

                    this.SetUpItunes();
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

                this.separatorFormat = Convert.ToString(registryKey.GetValue("Separator Format", "―"));

                this.artistFormat = Convert.ToString(registryKey.GetValue("Artist Format", "$a"));

                registryKey.Close();
            }
        }

        private void SaveSettings()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(
                string.Format(
                    "SOFTWARE\\{0}\\{1}\\{2}",
                    this.company,
                    this.application,
                    this.version));

            // Since this application supports only iTunes or Spotify we can
            // use a single value and check whether it's true or false.
            if (this.toolStripMenuItemSpotify.Checked)
            {
                registryKey.SetValue("Spotify", "true");
            }
            else
            {
                registryKey.SetValue("Spotify", "false");
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

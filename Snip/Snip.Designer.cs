﻿namespace Winter
{
    public partial class Snip
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSnipVersion;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSpotify;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemItunes;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWinamp;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFoobar2000;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemVlc;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMpsyt;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSetFormat;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveSeparateFiles;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveAlbumArtwork;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemKeepSpotifyAlbumArtwork;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTiny;
        // private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSmall; // Disabling the small size since no small size is provided via Spotify
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMedium;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLarge;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveHistory;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDisplayTrackPopup;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEmptyFileIfNoTrackPlaying;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEnableHotkeys;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExit;
        private System.Windows.Forms.Timer timerScanMediaPlayer;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            if (disposing && (this.keyboardHook != null))
            {
                this.keyboardHook.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSnipVersion = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSpotify = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemItunes = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemWinamp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemFoobar2000 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemVlc = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMpsyt = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSetFormat = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSaveSeparateFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSaveAlbumArtwork = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemKeepSpotifyAlbumArtwork = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemTiny = new System.Windows.Forms.ToolStripMenuItem();
            // this.toolStripMenuItemSmall = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMedium = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemLarge = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSaveHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDisplayTrackPopup = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemEmptyFileIfNoTrackPlaying = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemEnableHotkeys = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.timerScanMediaPlayer = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Text = Globals.ResourceManager.GetString("NoTrackPlaying");
            this.notifyIcon.Visible = true;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSnipVersion,
            this.toolStripSeparator,
            this.toolStripMenuItemSpotify,
            this.toolStripMenuItemItunes,
            this.toolStripMenuItemWinamp,
            this.toolStripMenuItemFoobar2000,
            this.toolStripMenuItemVlc,
            this.toolStripMenuItemMpsyt,
            this.toolStripSeparator1,
            this.toolStripMenuItemSetFormat,
            this.toolStripSeparator2,
            this.toolStripMenuItemSaveSeparateFiles,
            this.toolStripMenuItemSaveAlbumArtwork,
            this.toolStripMenuItemKeepSpotifyAlbumArtwork,
            this.toolStripMenuItemSaveHistory,
            this.toolStripMenuItemDisplayTrackPopup,
            this.toolStripMenuItemEmptyFileIfNoTrackPlaying,
            this.toolStripMenuItemEnableHotkeys,
            this.toolStripSeparator3,
            this.toolStripMenuItemExit});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(68, 220);
            // 
            // toolStripMenuItemSnipVersion
            // 
            this.toolStripMenuItemSnipVersion.Enabled = false;
            this.toolStripMenuItemSnipVersion.Name = "toolStripMenuItemSnipVersion";
            this.toolStripMenuItemSnipVersion.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemSnipVersion.Text = Globals.ResourceManager.GetString("SnipForm") + AssemblyInformation.AssemblyShorterVersion;
            // 
            // toolStripMenuItemSpotify
            // 
            this.toolStripMenuItemSpotify.Checked = true;
            this.toolStripMenuItemSpotify.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemSpotify.Name = "toolStripMenuItemSpotify";
            this.toolStripMenuItemSpotify.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemSpotify.Text = Globals.ResourceManager.GetString("Spotify");
            this.toolStripMenuItemSpotify.Click += new System.EventHandler(this.PlayerSelectionCheck);
            // 
            // toolStripMenuItemItunes
            // 
            this.toolStripMenuItemItunes.Name = "toolStripMenuItemItunes";
            this.toolStripMenuItemItunes.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemItunes.Text = Globals.ResourceManager.GetString("iTunes");
            this.toolStripMenuItemItunes.Click += new System.EventHandler(this.PlayerSelectionCheck);
            // 
            // toolStripMenuItemWinamp
            // 
            this.toolStripMenuItemWinamp.Name = "toolStripMenuItemWinamp";
            this.toolStripMenuItemWinamp.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemWinamp.Text = Globals.ResourceManager.GetString("Winamp");
            this.toolStripMenuItemWinamp.Click += new System.EventHandler(this.PlayerSelectionCheck);
            // 
            // toolStripMenuItemFoobar2000
            // 
            this.toolStripMenuItemFoobar2000.Name = "toolStripMenuItemFoobar2000";
            this.toolStripMenuItemFoobar2000.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemFoobar2000.Text = Globals.ResourceManager.GetString("foobar2000");
            this.toolStripMenuItemFoobar2000.Click += new System.EventHandler(this.PlayerSelectionCheck);
            // 
            // toolStripMenuItemVlc
            // 
            this.toolStripMenuItemVlc.Name = "toolStripMenuItemVlc";
            this.toolStripMenuItemVlc.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemVlc.Text = Globals.ResourceManager.GetString("VLC");
            this.toolStripMenuItemVlc.Click += new System.EventHandler(this.PlayerSelectionCheck);
            // 
            // toolStripMenuItemMpsyt
            // 
            this.toolStripMenuItemMpsyt.Name = "toolStripMenuItemMpsyt";
            this.toolStripMenuItemMpsyt.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemMpsyt.Text = Globals.ResourceManager.GetString("mpsyt");
            this.toolStripMenuItemMpsyt.Click += new System.EventHandler(this.PlayerSelectionCheck);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(64, 6);
            // 
            // toolStripMenuItemSetFormat
            // 
            this.toolStripMenuItemSetFormat.Name = "toolStripMenuItemSetFormat";
            this.toolStripMenuItemSetFormat.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemSetFormat.Text = Globals.ResourceManager.GetString("SetOutputFormat");
            this.toolStripMenuItemSetFormat.Click += new System.EventHandler(this.ToolStripMenuItemSetFormat_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(64, 6);
            // 
            // toolStripMenuItemSaveSeparateFiles
            // 
            this.toolStripMenuItemSaveSeparateFiles.Name = "toolStripMenuItemSaveSeparateFiles";
            this.toolStripMenuItemSaveSeparateFiles.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemSaveSeparateFiles.Text = Globals.ResourceManager.GetString("SaveInformationSeparately");
            this.toolStripMenuItemSaveSeparateFiles.Click += new System.EventHandler(this.ToolStripMenuItemSaveSeparateFiles_Click);
            // 
            // toolStripMenuItemSaveAlbumArtwork
            // 
            this.toolStripMenuItemSaveAlbumArtwork.Name = "toolStripMenuItemSaveAlbumArtwork";
            this.toolStripMenuItemSaveAlbumArtwork.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemSaveAlbumArtwork.Text = Globals.ResourceManager.GetString("SaveAlbumArtwork");
            this.toolStripMenuItemSaveAlbumArtwork.Click += new System.EventHandler(this.ToolStripMenuItemSaveAlbumArtwork_Click);
            // 
            // toolStripMenuItemKeepSpotifyAlbumArtwork
            // 
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemTiny,
            // this.toolStripMenuItemSmall,
            this.toolStripMenuItemMedium,
            this.toolStripMenuItemLarge});
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.Name = "toolStripMenuItemKeepSpotifyAlbumArtwork";
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.Text = Globals.ResourceManager.GetString("KeepSpotifyAlbumArtwork");
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.Click += new System.EventHandler(this.ToolStripMenuItemKeepSpotifyAlbumArtwork_Click);
            // 
            // toolStripMenuItemTiny
            // 
            this.toolStripMenuItemTiny.Checked = true;
            this.toolStripMenuItemTiny.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemTiny.Name = "toolStripMenuItemTiny";
            this.toolStripMenuItemTiny.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemTiny.Text = Globals.ResourceManager.GetString("ImageResolutionTiny");
            this.toolStripMenuItemTiny.Click += new System.EventHandler(this.AlbumArtworkResolutionCheck);
            // 
            // toolStripMenuItemSmall
            // 
            // this.toolStripMenuItemSmall.Name = "toolStripMenuItemSmall";
            // this.toolStripMenuItemSmall.Size = new System.Drawing.Size(67, 22);
            // this.toolStripMenuItemSmall.Text = Globals.ResourceManager.GetString("ImageResolutionSmall");
            // this.toolStripMenuItemSmall.Click += new System.EventHandler(this.AlbumArtworkResolutionCheck);
            // 
            // toolStripMenuItemMedium
            // 
            this.toolStripMenuItemMedium.Name = "toolStripMenuItemMedium";
            this.toolStripMenuItemMedium.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemMedium.Text = Globals.ResourceManager.GetString("ImageResolutionMedium");
            this.toolStripMenuItemMedium.Click += new System.EventHandler(this.AlbumArtworkResolutionCheck);
            // 
            // toolStripMenuItemLarge
            // 
            this.toolStripMenuItemLarge.Name = "toolStripMenuItemLarge";
            this.toolStripMenuItemLarge.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemLarge.Text = Globals.ResourceManager.GetString("ImageResolutionLarge");
            this.toolStripMenuItemLarge.Click += new System.EventHandler(this.AlbumArtworkResolutionCheck);
            // 
            // toolStripMenuItemSaveHistory
            // 
            this.toolStripMenuItemSaveHistory.Name = "toolStripMenuItemSaveHistory";
            this.toolStripMenuItemSaveHistory.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemSaveHistory.Text = Globals.ResourceManager.GetString("SaveTrackHistory");
            this.toolStripMenuItemSaveHistory.Click += new System.EventHandler(this.ToolStripMenuItemSaveHistory_Click);
            // 
            // toolStripMenuItemDisplayTrackPopup
            // 
            this.toolStripMenuItemDisplayTrackPopup.Name = "toolStripMenuItemDisplayTrackPopup";
            this.toolStripMenuItemDisplayTrackPopup.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemDisplayTrackPopup.Text = Globals.ResourceManager.GetString("DisplayTrackPopup");
            this.toolStripMenuItemDisplayTrackPopup.Click += new System.EventHandler(this.ToolStripMenuItemDisplayTrackPopup_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(64, 6);
            // 
            // toolStripMenuItemExit
            // 
            this.toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            this.toolStripMenuItemExit.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemExit.Text = Globals.ResourceManager.GetString("ExitApplication");
            this.toolStripMenuItemExit.Click += new System.EventHandler(this.ToolStripMenuItemExit_Click);
            // 
            // toolStripMenuItemEmptyFileIfNoTrackPlaying
            // 
            this.toolStripMenuItemEmptyFileIfNoTrackPlaying.Checked = true;
            this.toolStripMenuItemEmptyFileIfNoTrackPlaying.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemEmptyFileIfNoTrackPlaying.Name = "toolStripMenuItemEmptyFileIfNoTrackPlaying";
            this.toolStripMenuItemEmptyFileIfNoTrackPlaying.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemEmptyFileIfNoTrackPlaying.Text = Globals.ResourceManager.GetString("EmptyFile");
            this.toolStripMenuItemEmptyFileIfNoTrackPlaying.Click += new System.EventHandler(this.ToolStripMenuItemEmptyFileIfNoTrackPlaying_Click);
            // 
            // toolStripMenuItemEnableHotkeys
            // 
            this.toolStripMenuItemEnableHotkeys.Checked = true;
            this.toolStripMenuItemEnableHotkeys.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemEnableHotkeys.Name = "toolStripMenuItemEnableHotkeys";
            this.toolStripMenuItemEnableHotkeys.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemEnableHotkeys.Text = Globals.ResourceManager.GetString("EnableHotkeys");
            this.toolStripMenuItemEnableHotkeys.Click += new System.EventHandler(this.ToolStripMenuItemEnableHotkeys_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator.Name = "toolStripSeparator3";
            this.toolStripSeparator.Size = new System.Drawing.Size(64, 6);
            // 
            // timerScanTitle
            // 
            this.timerScanMediaPlayer.Enabled = false;
            this.timerScanMediaPlayer.Tick += new System.EventHandler(this.TimerScanMediaPlayer_Tick);
            // 
            // Snip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Icon = Properties.Resources.SnipIcon;
            this.Name = "Snip";
            this.Text = Globals.ResourceManager.GetString("SnipForm");
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

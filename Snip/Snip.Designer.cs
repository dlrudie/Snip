namespace Winter
{
    public partial class Snip
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemItunes;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSpotify;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExit;
        private System.Windows.Forms.Timer timerScanTitle;
        private System.Windows.Forms.Timer timerHotkey;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveHistory;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveSeparateFiles;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveAlbumArtwork;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSetFormat;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWinamp;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemKeepSpotifyAlbumArtwork;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTiny;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSmall;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMedium;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLarge;

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
            this.toolStripMenuItemSpotify = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemItunes = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemWinamp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSetFormat = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSaveSeparateFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSaveAlbumArtwork = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemKeepSpotifyAlbumArtwork = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemTiny = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSmall = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMedium = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemLarge = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSaveHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.timerScanTitle = new System.Windows.Forms.Timer(this.components);
            this.timerHotkey = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Text = this.resourceManager.GetString("NoTrackPlaying");
            this.notifyIcon.Visible = true;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSpotify,
            this.toolStripMenuItemItunes,
            this.toolStripMenuItemWinamp,
            this.toolStripSeparator,
            this.toolStripMenuItemSetFormat,
            this.toolStripSeparator2,
            this.toolStripMenuItemSaveSeparateFiles,
            this.toolStripMenuItemSaveAlbumArtwork,
            this.toolStripMenuItemKeepSpotifyAlbumArtwork,
            this.toolStripMenuItemSaveHistory,
            this.toolStripSeparator1,
            this.toolStripMenuItemExit});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(68, 220);
            // 
            // toolStripMenuItemSpotify
            // 
            this.toolStripMenuItemSpotify.Checked = true;
            this.toolStripMenuItemSpotify.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemSpotify.Name = "toolStripMenuItemSpotify";
            this.toolStripMenuItemSpotify.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemSpotify.Text = this.resourceManager.GetString("Spotify");
            this.toolStripMenuItemSpotify.Click += new System.EventHandler(this.ToolStripMenuItemSpotify_Click);
            // 
            // toolStripMenuItemItunes
            // 
            this.toolStripMenuItemItunes.Name = "toolStripMenuItemItunes";
            this.toolStripMenuItemItunes.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemItunes.Text = this.resourceManager.GetString("iTunes");
            this.toolStripMenuItemItunes.Click += new System.EventHandler(this.ToolStripMenuItemItunes_Click);
            // 
            // toolStripMenuItemWinamp
            // 
            this.toolStripMenuItemWinamp.Name = "toolStripMenuItemWinamp";
            this.toolStripMenuItemWinamp.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemWinamp.Text = this.resourceManager.GetString("Winamp");
            this.toolStripMenuItemWinamp.Click += new System.EventHandler(this.ToolStripMenuItemWinamp_Click);
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
            this.toolStripMenuItemSetFormat.Text = this.resourceManager.GetString("SetOutputFormat");
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
            this.toolStripMenuItemSaveSeparateFiles.Text = this.resourceManager.GetString("SaveInformationSeparately");
            this.toolStripMenuItemSaveSeparateFiles.Click += new System.EventHandler(this.ToolStripMenuItemSaveSeparateFiles_Click);
            // 
            // toolStripMenuItemSaveAlbumArtwork
            // 
            this.toolStripMenuItemSaveAlbumArtwork.Name = "toolStripMenuItemSaveAlbumArtwork";
            this.toolStripMenuItemSaveAlbumArtwork.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemSaveAlbumArtwork.Text = this.resourceManager.GetString("SaveAlbumArtwork");
            this.toolStripMenuItemSaveAlbumArtwork.Click += new System.EventHandler(this.ToolStripMenuItemSaveAlbumArtwork_Click);
            // 
            // toolStripMenuItemKeepSpotifyAlbumArtwork
            // 
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.Checked = true;
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemTiny,
            this.toolStripMenuItemSmall,
            this.toolStripMenuItemMedium,
            this.toolStripMenuItemLarge});
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.Name = "toolStripMenuItemKeepSpotifyAlbumArtwork";
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.Text = this.resourceManager.GetString("KeepSpotifyAlbumArtwork");
            this.toolStripMenuItemKeepSpotifyAlbumArtwork.Click += new System.EventHandler(this.ToolStripMenuItemKeepSpotifyAlbumArtwork_Click);
            // 
            // toolStripMenuItemTiny
            // 
            this.toolStripMenuItemTiny.Checked = true;
            this.toolStripMenuItemTiny.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemTiny.Name = "toolStripMenuItemTiny";
            this.toolStripMenuItemTiny.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemTiny.Text = this.resourceManager.GetString("ImageResolutionTiny");
            this.toolStripMenuItemTiny.Click += new System.EventHandler(this.AlbumArtworkResolutionCheck);
            // 
            // toolStripMenuItemSmall
            // 
            this.toolStripMenuItemSmall.Name = "toolStripMenuItemSmall";
            this.toolStripMenuItemSmall.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemSmall.Text = this.resourceManager.GetString("ImageResolutionSmall");
            this.toolStripMenuItemSmall.Click += new System.EventHandler(this.AlbumArtworkResolutionCheck);
            // 
            // toolStripMenuItemMedium
            // 
            this.toolStripMenuItemMedium.Name = "toolStripMenuItemMedium";
            this.toolStripMenuItemMedium.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemMedium.Text = this.resourceManager.GetString("ImageResolutionMedium");
            this.toolStripMenuItemMedium.Click += new System.EventHandler(this.AlbumArtworkResolutionCheck);
            // 
            // toolStripMenuItemLarge
            // 
            this.toolStripMenuItemLarge.Name = "toolStripMenuItemLarge";
            this.toolStripMenuItemLarge.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemLarge.Text = this.resourceManager.GetString("ImageResolutionLarge");
            this.toolStripMenuItemLarge.Click += new System.EventHandler(this.AlbumArtworkResolutionCheck);
            // 
            // toolStripMenuItemSaveHistory
            // 
            this.toolStripMenuItemSaveHistory.Name = "toolStripMenuItemSaveHistory";
            this.toolStripMenuItemSaveHistory.Size = new System.Drawing.Size(67, 22);
            this.toolStripMenuItemSaveHistory.Text = this.resourceManager.GetString("SaveTrackHistory");
            this.toolStripMenuItemSaveHistory.Click += new System.EventHandler(this.ToolStripMenuItemSaveHistory_Click);
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
            this.toolStripMenuItemExit.Text = this.resourceManager.GetString("ExitApplication");
            this.toolStripMenuItemExit.Click += new System.EventHandler(this.ToolStripMenuItemExit_Click);
            // 
            // timerScanTitle
            // 
            this.timerScanTitle.Enabled = true;
            this.timerScanTitle.Tick += new System.EventHandler(this.TimerScanTitle_Tick);
            // 
            // timerHotkey
            // 
            this.timerHotkey.Enabled = true;
            this.timerHotkey.Interval = 1;
            this.timerHotkey.Tick += new System.EventHandler(this.TimerHotkey_Tick);
            // 
            // Snip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "Snip";
            this.Text = this.resourceManager.GetString("SnipForm");
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

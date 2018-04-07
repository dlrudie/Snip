namespace Winter
{
    partial class OutputFormat
    {
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.labelTrackFormat = new System.Windows.Forms.Label();
            this.textBoxTrackFormat = new System.Windows.Forms.TextBox();
            this.labelSeparatorFormat = new System.Windows.Forms.Label();
            this.textBoxSeparatorFormat = new System.Windows.Forms.TextBox();
            this.labelArtistFormat = new System.Windows.Forms.Label();
            this.textBoxArtistFormat = new System.Windows.Forms.TextBox();
            this.buttonDefaults = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.labelAlbumFormat = new System.Windows.Forms.Label();
            this.textBoxAlbumFormat = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelTrackFormat
            // 
            this.labelTrackFormat.Location = new System.Drawing.Point(12, 9);
            this.labelTrackFormat.Name = "labelTrackFormat";
            this.labelTrackFormat.Size = new System.Drawing.Size(254, 13);
            this.labelTrackFormat.TabIndex = 0;
            this.labelTrackFormat.Text = LocalizedMessages.SetTrackFormat;
            // 
            // textBoxTrackFormat
            // 
            this.textBoxTrackFormat.Location = new System.Drawing.Point(12, 25);
            this.textBoxTrackFormat.Name = "textBoxTrackFormat";
            this.textBoxTrackFormat.Size = new System.Drawing.Size(254, 20);
            this.textBoxTrackFormat.TabIndex = 1;
            this.textBoxTrackFormat.Text = LocalizedMessages.TrackFormat;
            // 
            // labelSeparatorFormat
            // 
            this.labelSeparatorFormat.Location = new System.Drawing.Point(12, 58);
            this.labelSeparatorFormat.Name = "labelSeparatorFormat";
            this.labelSeparatorFormat.Size = new System.Drawing.Size(254, 13);
            this.labelSeparatorFormat.TabIndex = 2;
            this.labelSeparatorFormat.Text = LocalizedMessages.SetSeparatorFormat;
            // 
            // textBoxSeparatorFormat
            // 
            this.textBoxSeparatorFormat.Location = new System.Drawing.Point(12, 74);
            this.textBoxSeparatorFormat.Name = "textBoxSeparatorFormat";
            this.textBoxSeparatorFormat.Size = new System.Drawing.Size(254, 20);
            this.textBoxSeparatorFormat.TabIndex = 3;
            this.textBoxSeparatorFormat.Text = LocalizedMessages.SeparatorFormat;
            // 
            // labelArtistFormat
            // 
            this.labelArtistFormat.Location = new System.Drawing.Point(12, 107);
            this.labelArtistFormat.Name = "labelArtistFormat";
            this.labelArtistFormat.Size = new System.Drawing.Size(254, 13);
            this.labelArtistFormat.TabIndex = 4;
            this.labelArtistFormat.Text = LocalizedMessages.SetArtistFormat;
            // 
            // textBoxArtistFormat
            // 
            this.textBoxArtistFormat.Location = new System.Drawing.Point(12, 123);
            this.textBoxArtistFormat.Name = "textBoxArtistFormat";
            this.textBoxArtistFormat.Size = new System.Drawing.Size(254, 20);
            this.textBoxArtistFormat.TabIndex = 5;
            this.textBoxArtistFormat.Text = LocalizedMessages.ArtistFormat;
            // 
            // buttonDefaults
            // 
            this.buttonDefaults.Location = new System.Drawing.Point(12, 206);
            this.buttonDefaults.Name = "buttonDefaults";
            this.buttonDefaults.Size = new System.Drawing.Size(122, 23);
            this.buttonDefaults.TabIndex = 8;
            this.buttonDefaults.Text = LocalizedMessages.ButtonDefaults;
            this.buttonDefaults.UseVisualStyleBackColor = true;
            this.buttonDefaults.Click += new System.EventHandler(this.ButtonDefaults_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(144, 206);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(122, 23);
            this.buttonSave.TabIndex = 9;
            this.buttonSave.Text = LocalizedMessages.ButtonSave;
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // labelAlbumFormat
            // 
            this.labelAlbumFormat.Location = new System.Drawing.Point(12, 155);
            this.labelAlbumFormat.Name = "labelAlbumFormat";
            this.labelAlbumFormat.Size = new System.Drawing.Size(254, 13);
            this.labelAlbumFormat.TabIndex = 6;
            this.labelAlbumFormat.Text = LocalizedMessages.SetAlbumFormat;
            // 
            // textBoxAlbumFormat
            // 
            this.textBoxAlbumFormat.Location = new System.Drawing.Point(12, 171);
            this.textBoxAlbumFormat.Name = "textBoxAlbumFormat";
            this.textBoxAlbumFormat.Size = new System.Drawing.Size(254, 20);
            this.textBoxAlbumFormat.TabIndex = 7;
            this.textBoxAlbumFormat.Text = LocalizedMessages.AlbumFormat;
            // 
            // OutputFormat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(278, 241);
            this.Controls.Add(this.textBoxAlbumFormat);
            this.Controls.Add(this.labelAlbumFormat);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonDefaults);
            this.Controls.Add(this.textBoxArtistFormat);
            this.Controls.Add(this.labelArtistFormat);
            this.Controls.Add(this.textBoxSeparatorFormat);
            this.Controls.Add(this.labelSeparatorFormat);
            this.Controls.Add(this.textBoxTrackFormat);
            this.Controls.Add(this.labelTrackFormat);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OutputFormat";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = LocalizedMessages.SetOutputFormat;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTrackFormat;
        private System.Windows.Forms.TextBox textBoxTrackFormat;
        private System.Windows.Forms.Label labelSeparatorFormat;
        private System.Windows.Forms.TextBox textBoxSeparatorFormat;
        private System.Windows.Forms.Label labelArtistFormat;
        private System.Windows.Forms.TextBox textBoxArtistFormat;
        private System.Windows.Forms.Button buttonDefaults;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Label labelAlbumFormat;
        private System.Windows.Forms.TextBox textBoxAlbumFormat;
    }
}
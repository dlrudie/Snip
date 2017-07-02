namespace Winter
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    internal sealed class mpsyt : MediaPlayer
    {
        public override void Update()
        {

            Process[] processes = Process.GetProcessesByName("mpsyt");

            if (processes.Length > 0)
            {
                string windowTitle = string.Empty;

                foreach (Process process in processes)
                {
                    windowTitle = process.MainWindowTitle;
                    this.Handle = process.MainWindowHandle;
                }

                // We need to check if the title exists,
                // if mpsyt was ran from console there will exist 
                if(windowTitle.Length > 0)
                {
                    // Remove the '- mpsyt' part of the window title to recive the song title
                    int lastHyphen = windowTitle.LastIndexOf("-", StringComparison.OrdinalIgnoreCase);

                    //If we find a hyphen a track is playing there is no track playing
                    if (lastHyphen > 0)
                    {
                        string songTitle = windowTitle.Substring(0, lastHyphen).Trim();

                        if (Globals.SaveAlbumArtwork)
                        {
                            this.SaveBlankImage();
                        }

                        TextHandler.UpdateText(songTitle);
                    }
                    else
                    {
                        TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("NoTrackPlaying"));
                    }
                }
                else
                {
                    TextHandler.UpdateTextAndEmptyFilesMaybe(Globals.ResourceManager.GetString("MpsNoTitleFound"));
                }
            

            }
        }

        public override void Unload()
        {
            base.Unload();
        }

        public override void ChangeToNextTrack()
        {
            //We need to sleep a few ms to wait for the ctrl+alt to be depressed
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)'>', IntPtr.Zero);
        }

        public override void ChangeToPreviousTrack()
        {
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)'<', IntPtr.Zero);
        }

        public override void IncreasePlayerVolume()
        {
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)'0', IntPtr.Zero);
        }

        public override void DecreasePlayerVolume()
        {
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)'9', IntPtr.Zero);
        }

        public override void PlayOrPauseTrack()
        {
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)' ', IntPtr.Zero);
        }

        public override void StopTrack()
        {
            Thread.Sleep(Globals.wmCharDelay);
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.Char, (IntPtr)'q', IntPtr.Zero);
        }
    }
}

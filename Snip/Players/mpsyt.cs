using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winter
{
    using System;
    using System.Diagnostics;

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
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.NextTrack));
        }

        public override void ChangeToPreviousTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.PreviousTrack));
        }

        public override void IncreasePlayerVolume()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.VolumeUp));
        }

        public override void DecreasePlayerVolume()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.VolumeDown));
        }

        public override void MutePlayerAudio()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.MuteTrack));
        }

        public override void PlayOrPauseTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.PlayPauseTrack));
        }

        public override void StopTrack()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, (uint)Globals.WindowMessage.AppCommand, IntPtr.Zero, new IntPtr((long)Globals.MediaCommand.StopTrack));
        }
    }
}

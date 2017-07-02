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

                System.Console.WriteLine(windowTitle);

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

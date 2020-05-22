using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows_Media_Controller_Library.Models.Data;

namespace Windows_Media_Controller_Library.Modules
{
    public class MusicCommandHandler
    {
		private const int WM_APPCOMMAND = 0x319;
		private const int SC_MONITORPOWER = 0xF170;
		private const int WM_SYSCOMMAND = 0x0112;

		private VolumeController audio;
		private MusicController music;
		public MusicCommandHandler()
		{
			audio = new VolumeController();
			music = new MusicController();
		}

		public bool InvokeCommand(Client c, TransferCommandObject obj)
        {
            switch (obj.Command)
            {
                case "VolumeUp":
					audio.VolumeUp(2);
					return true;
                case "VolumeDown":
					audio.VolumeDown(2);
					return true;
				case "VolumeMute":
					audio.VolumeMute();
                    break;
				case "Pause":
					music.PlayPause();
					break;
				case "Stop":
                    break;
                case "SkipNext":
					music.Next();
                    break;
                case "SkipPrev":
					music.Prev();
                    break;
				case "ScreenBlack":
					MonitorSleep();
					return true;
			}

			return false;
        }



		[DllImport("user32.dll")]
		public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);	

		public void MonitorSleep()
		{
			SendMessageW(GetHandle(), WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)2);
		}

		private static IntPtr GetHandle()
		{
			return Process.GetCurrentProcess().MainWindowHandle;
		}
	}
}

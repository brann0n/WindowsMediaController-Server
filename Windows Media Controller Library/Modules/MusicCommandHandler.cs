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
		private const int SC_MONITORPOWER = 0xF170;
		private const int WM_SYSCOMMAND = 0x0112;

		private readonly VolumeController Audio;
		private readonly MusicController Music;
		public MusicCommandHandler()
		{
			Audio = new VolumeController();
			Music = new MusicController();
		}

		public bool InvokeCommand(Client c, TransferCommandObject obj)
        {
            switch (obj.Command)
            {
                case "VolumeUp":
					Audio.VolumeUp(int.Parse(obj.Value));
					return true;
                case "VolumeDown":
					Audio.VolumeDown(int.Parse(obj.Value));
					return true;
				case "VolumeMute":
					Audio.VolumeMute();
					return true;
				case "Pause":
					Music.PlayPause();
					return true;
				case "Stop":
                    break;
                case "SkipNext":
					Music.Next();
					return true;
				case "SkipPrev":
					Music.Prev();
					return true;
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

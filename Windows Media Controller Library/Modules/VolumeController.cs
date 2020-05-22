using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Media_Controller_Library.Modules
{
    public class VolumeController
    {
		private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
		private const int APPCOMMAND_VOLUME_UP = 0xA0000;
		private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
		private const int WM_APPCOMMAND = 0x319;

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		public void VolumeUp(int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				SendMessageW(GetHandle(), WM_APPCOMMAND, GetHandle(), (IntPtr)APPCOMMAND_VOLUME_UP);
			}
		}

		public void VolumeDown(int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				SendMessageW(GetHandle(), WM_APPCOMMAND, GetHandle(), (IntPtr)APPCOMMAND_VOLUME_DOWN);
			}
		}

		public void VolumeMute()
		{
			SendMessageW(GetHandle(), WM_APPCOMMAND, GetHandle(), (IntPtr)APPCOMMAND_VOLUME_MUTE);
		}

		private static IntPtr GetHandle()
		{
			return Process.GetCurrentProcess().MainWindowHandle;
		}
	}
}

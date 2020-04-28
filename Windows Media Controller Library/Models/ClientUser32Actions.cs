using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows_Media_Controller_Library.Modules;

namespace Windows_Media_Controller_Library.Models
{
	[Serializable]
	public class ClientUser32Action : IExecuteInterface
	{
		private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
		private const int APPCOMMAND_VOLUME_UP = 0xA0000;
		private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
		private const int WM_APPCOMMAND = 0x319;
		private const int SC_MONITORPOWER = 0xF170;
		private const int WM_SYSCOMMAND = 0x0112;

		private string action;
		public ClientUser32Action(string Action)
		{
			action = Action;
		}

		public void Execute()
		{
			switch (action)
			{
				case "VolumeUp":
					VolumeUp(5);
					break;
				case "VolumeDown":
					VolumeDown(5);
					break;
				case "VolumeMute":
					VolumeMute();
					break;
				case "MonitorSleep":
					MonitorSleep();
					break;
				case "PressWindowsKey":
					break;
			}
		}

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

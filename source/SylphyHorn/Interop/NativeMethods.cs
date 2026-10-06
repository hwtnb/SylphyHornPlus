using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MetroRadiance.Interop.Win32;

namespace SylphyHorn.Interop
{
	public static class NativeMethods
	{
		[DllImport("user32.dll")]
		public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
		
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetCursorPos(out POINT lpPoint);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr MonitorFromPoint(POINT pt, MonitorDefaultTo dwFlags);

		[DllImport("user32.dll")]
		public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorDefaultTo dwFlags);

		[DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, LayeredWindowAttributes dwFlags);

		[DllImport("dxva2.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref int pdwNumberOfPhysicalMonitors);

		[DllImport("dxva2.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

		[DllImport("dxva2.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, PHYSICAL_MONITOR[] pPhysicalMonitorArray);

		public static class GlobalHook
		{
			public const int WH_KEYBOARD_LL = 13;
			public const int HC_ACTION = 0;

			public const int WM_KEYDOWN = 0x0100;
			public const int WM_KEYUP = 0x0101;
			public const int WM_SYSKEYDOWN = 0x0104;
			public const int WM_SYSKEYUP = 0x0105;

			public const uint LLKHF_EXTENDED = 0x01;
			public const uint LLKHF_INJECTED = 0x10;

			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern IntPtr GetModuleHandle(string moduleName);

			[DllImport("user32.dll")]
			public static extern short GetAsyncKeyState(int vKey);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern IntPtr SetWindowsHookEx(int idHook, HookDelegate lpfn, IntPtr hMod, uint dwThreadId);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookDelegate lpfn, IntPtr hMod, uint dwThreadId);

			[DllImport("user32.dll")]
			public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, uint msg, ref MSLLHOOKSTRUCT msllhookstruct);

			[DllImport("user32.dll")]
			public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

			[DllImport("user32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool UnhookWindowsHookEx(IntPtr hhk);

			public delegate IntPtr HookDelegate(int nCode, uint msg, ref MSLLHOOKSTRUCT msllhookstruct);
			public delegate IntPtr KeyboardHookDelegate(int nCode, IntPtr wParam, IntPtr lParam);
		}
	}

	public delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);
}

// From http://stackoverflow.com/a/24187171/1641069

using System;
using System.Runtime.InteropServices;

internal static class TaskbarProgress
{
	internal enum TaskbarStates
	{
		NoProgress = 0,
		Indeterminate = 0x1,
		Normal = 0x2,
		Error = 0x4,
		Paused = 0x8
	}

	[ComImport()]
	[Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
#pragma warning disable SYSLIB1096 // Convert to 'GeneratedComInterface'
	private interface ITaskbarList3
#pragma warning restore SYSLIB1096 // Convert to 'GeneratedComInterface'
	{
		// ITaskbarList
		[PreserveSig]
		void HrInit();
		[PreserveSig]
		void AddTab(IntPtr hwnd);
		[PreserveSig]
		void DeleteTab(IntPtr hwnd);
		[PreserveSig]
		void ActivateTab(IntPtr hwnd);
		[PreserveSig]
		void SetActiveAlt(IntPtr hwnd);

		// ITaskbarList2
		[PreserveSig]
		void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

		// ITaskbarList3
		[PreserveSig]
		void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
		[PreserveSig]
		void SetProgressState(IntPtr hwnd, TaskbarStates state);
	}

	[Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
	[ClassInterface(ClassInterfaceType.None)]
	[ComImport()]
	private class TaskbarInstance
	{
	}

	private static readonly ITaskbarList3 taskbarInstance = (ITaskbarList3)new TaskbarInstance();
	private static readonly bool taskbarSupported = Environment.OSVersion.Version >= new Version(6, 1);

	internal static void SetState(IntPtr windowHandle, TaskbarStates taskbarState)
	{
		if (taskbarSupported)
			taskbarInstance.SetProgressState(windowHandle, taskbarState);
	}

	internal static void SetValue(IntPtr windowHandle, double progressValue, double progressMax)
	{
		if (taskbarSupported)
			taskbarInstance.SetProgressValue(windowHandle, (ulong)progressValue, (ulong)progressMax);
	}
}
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using WitcherScriptMerger.Forms;
using WitcherScriptMerger.Inventory;
using WitcherScriptMerger.LoadOrder;
using WitcherScriptMerger.Theming;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
namespace WitcherScriptMerger;

internal static partial class Program
{
	#region Multiple instance handling

	private static readonly Mutex mutex = new(true, "{B2B9C9EA-E9D0-47EB-875E-854FCC925F19}");
	private const string pipeName = "SM-FAE_Pipe";
	private static readonly TimeSpan PipeTimeout = TimeSpan.FromSeconds(5); // Connection timeout for the pipe
	private static NamedPipeServerStream server;

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetForegroundWindow(IntPtr hWnd);

	/// <summary>
	/// Listens for a signal on a named pipe and brings the application window to the foreground.
	/// </summary>
	private static void ListenForSignal()
	{
		try
		{
			server = new(pipeName);
			while (true)
			{
				server.WaitForConnection();
				if (server.IsConnected)
				{
					db("Server connected!");
					Process currentProcess = Process.GetCurrentProcess();
					IntPtr windowHandle = currentProcess.MainWindowHandle;
					if (windowHandle != IntPtr.Zero)
						_ = SetForegroundWindow(windowHandle);
					else
						db("ALERT: windowHandle was zero, can't call SetForegroundWindow.");
					server.Disconnect();
				}
			}
		}
		catch (IOException)
		{
			// Application is closing
		}
	}

	/// <summary>
	/// Sends a signal to an existing instance of the application via a named pipe.
	/// If the signal fails, starts a new instance.
	/// </summary>
	private static void SendSignal()
	{
		_ = MessageBox.Show("Another instance of SM-FAE is already running. Trying to switch to it now, but this won't work if it's minimized. If so, switch to it manually, or check Task Manager if an old WitcherScriptMerger.exe process is still running.\n\nIf you see this popup while no other instance is active, please report this as a bug!", "Another SM-FAE instance is running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

		using NamedPipeClientStream client = new(pipeName);
		try
		{
			client.Connect((int)PipeTimeout.TotalMilliseconds);
		}
		catch (Exception ex)
		{
			// The existing instance did not respond, continue starting the app 
			db(ex.ToString());
			RunApplication();
		}
	}

	/// <summary>
	/// Stops the named pipe server.
	/// </summary>
	internal static void StopListening() => server?.Dispose();

	#endregion

	internal static AppSettings Settings = new();
	internal static CustomLoadOrder LoadOrder;
	internal static MergeInventory Inventory;
	internal static MainForm MainFrm;
	internal static OptionsForm OptionsFrm;
	internal static MessageForm MessageFrm;
	internal static ThemeManager ThemeMngr;

	internal static string AppName = "Script Merger: Fresh & Automated Edition";

	#region Titlebar Follow Windows Theming
	// Title bar follow Windows Dark/Ligh theme
	// https://stackoverflow.com/questions/56312190/how-do-i-make-my-form-title-bar-follow-the-windows-dark-theme

	internal enum DWMWINDOWATTRIBUTE : uint
	{
		DWMWA_NCRENDERING_ENABLED,
		DWMWA_NCRENDERING_POLICY,
		DWMWA_TRANSITIONS_FORCEDISABLED,
		DWMWA_ALLOW_NCPAINT,
		DWMWA_CAPTION_BUTTON_BOUNDS,
		DWMWA_NONCLIENT_RTL_LAYOUT,
		DWMWA_FORCE_ICONIC_REPRESENTATION,
		DWMWA_FLIP3D_POLICY,
		DWMWA_EXTENDED_FRAME_BOUNDS,
		DWMWA_HAS_ICONIC_BITMAP,
		DWMWA_DISALLOW_PEEK,
		DWMWA_EXCLUDED_FROM_PEEK,
		DWMWA_CLOAK,
		DWMWA_CLOAKED,
		DWMWA_FREEZE_REPRESENTATION,
		DWMWA_PASSIVE_UPDATE_MODE,
		DWMWA_USE_HOSTBACKDROPBRUSH,
		DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
		DWMWA_WINDOW_CORNER_PREFERENCE = 33,
		DWMWA_BORDER_COLOR,
		DWMWA_CAPTION_COLOR,
		DWMWA_TEXT_COLOR,
		DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,
		DWMWA_SYSTEMBACKDROP_TYPE,
		DWMWA_LAST
	}

	[DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute'
	internal static extern void DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute,
		ref int pvAttribute, uint cbAttribute);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute'
	#endregion

	[STAThread]
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	private static void Main()
	{
		Application.EnableVisualStyles();
		_ = Application.SetHighDpiMode(HighDpiMode.SystemAware);
		Application.SetCompatibleTextRenderingDefault(false);

		try
		{
			// Attempt to acquire the mutex immediately.
			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				// If acquired, start the named pipe listener thread.
				Thread listenThread = new(ListenForSignal) { IsBackground = true };
				listenThread.Start();

				RunApplication();
				mutex.ReleaseMutex();
			}
			else
			{
				// If mutex is already held, send a signal to the existing instance.
				SendSignal();
			}
		}
		catch (Exception ex)
		{
			db(ex.ToString());
			throw;
		}
	}

	/// <summary>
	/// Initializes and runs the main application form.
	/// </summary>
	private static void RunApplication()
	{
		ThemeMngr = new();
		MainFrm = new();
		OptionsFrm = new();
		MessageFrm = new();

		Application.Run(MainFrm);
	}

	/// <summary>
	/// Updates the form's size and constraints based on the primary screen's working area.
	/// </summary>
	/// <param name="form">The form to update.</param>
	internal static void UpdateWindowSizeFromWorkingArea(Form form)
	{
		Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;

		form.MinimumSize = new Size(
			Math.Min(form.MinimumSize.Width, workingArea.Width),
			Math.Min(form.MinimumSize.Height, workingArea.Height));

		form.MaximumSize = new Size(
			Math.Max(form.MaximumSize.Width, form.MinimumSize.Width),
			Math.Max(form.MaximumSize.Height, form.MinimumSize.Height));

		form.Size = new Size(
			Math.Min(form.Width, workingArea.Width),
			Math.Min(form.Height, workingArea.Height));
	}

	/// <summary>
	/// Attempts to open a file. If the file is an executable, it runs it; otherwise, it opens the file's location.
	/// </summary>
	/// <param name="path">The path to the file.</param>
	/// <returns><see langword="true"/> if the file was found and opened successfully; otherwise, <see langword="false"/>.</returns>
	internal static bool TryOpenFile(string path)
	{
		if (!File.Exists(path))
		{
			_ = MainFrm.uiThreadManager.ShowMessage("Can't find file: " + path);
			return false;
		}

		if (path.EndsWithIgnoreCase(".exe"))
		{
			ProcessStartInfo startInfo = new()
			{
				FileName = path,
				WorkingDirectory = Path.GetDirectoryName(path)
			};
			_ = Process.Start(startInfo);
		}
		else
		{
			_ = TryOpenFileLocation(path);
		}

		return true;
	}

	/// <summary>
	/// Attempts to open the location of a file in File Explorer.
	/// </summary>
	/// <param name="filePath">The path to the file.</param>
	/// <returns><see langword="true"/> if the file exists and its location was opened; otherwise, <see langword="false"/>.</returns>
	internal static bool TryOpenFileLocation(string filePath)
	{
		if (File.Exists(filePath))
		{
			_ = Process.Start("explorer.exe", $"/select,\"{filePath}\"");
			return true;
		}

		_ = MainFrm.uiThreadManager.ShowMessage("Can't find file: " + filePath);
		return false;
	}

	/// <summary>
	/// Attempts to open a directory in File Explorer.
	/// </summary>
	/// <param name="dirPath">The path to the directory.</param>
	/// <returns><see langword="true"/> if the directory exists and was opened; otherwise, <see langword="false"/>.</returns>
	internal static bool TryOpenDirectory(string dirPath)
	{
		if (!Directory.Exists(dirPath))
		{
			_ = MainFrm.uiThreadManager.ShowMessage("Can't find directory: " + dirPath);
			return false;
		}

		_ = Process.Start("explorer.exe", dirPath);
		return true;
	}

	// Overload for TimerCallback compatibility
	internal static void AutoExit(object state) => AutoExit(state, null);

	/// <summary>
	/// Exits the application.
	/// </summary>
	/// <param name="state">Not used, but required by System.Threading.Timer which calls this method.</param>
	/// <param name="exitMessage">Optional message to show the user why the application is exiting.</param>
#pragma warning disable IDE0060 // Remove unused parameter
	internal static void AutoExit(object state = null, string exitMessage = null)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		if (!string.IsNullOrEmpty(exitMessage))
		{
			_ = MessageBox.Show(exitMessage, "Application Exiting", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		Settings.Save();
		Environment.Exit(0);
	}

	internal static void db(string msg) => Debug.WriteLine(msg);
}
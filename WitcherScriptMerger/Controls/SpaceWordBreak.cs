using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WitcherScriptMerger.Controls;

/// <summary>
/// Lets a rich text box break lines at spaces only. By default it also breaks after a backslash or a hyphen, so a path the
/// line had room for was split in two: "core" at the end of one line, "\math.ws" at the start of the next. A word longer
/// than the line is still broken wherever it must.
/// </summary>
internal static partial class SpaceWordBreak
{
	private const int EmSetWordBreakProc = 0x00D0;

	// The codes the control asks with (EDITWORDBREAKPROC).
	private const int WbIsDelimiter = 2;
	private const int WbClassify = 3;
	private const int WbLeft = 0;
	private const int WbMoveWordLeft = 4;
	private const int WbLeftBreak = 6;
	private const int WbfIsWhite = 0x10;
	private const int WbfBreakLine = 0x20;

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	private delegate int WordBreakProc(IntPtr text, int current, int length, int code);

	// Held for the life of the program: the control keeps a pointer to it.
	private static readonly WordBreakProc Proc = Break;
	private static readonly IntPtr ProcPointer = Marshal.GetFunctionPointerForDelegate(Proc);

	internal static void Apply(RichTextBox box)
	{
		ArgumentNullException.ThrowIfNull(box);
		if (box.IsHandleCreated)
			Set(box);
		box.HandleCreated += (sender, e) => Set((RichTextBox)sender);
	}

	private static void Set(RichTextBox box) => _ = SendMessageW(box.Handle, EmSetWordBreakProc, IntPtr.Zero, ProcPointer);

	// The text is UTF-16; current and length count characters.
	private static int Break(IntPtr text, int current, int length, int code)
	{
		bool IsSpace(int i) => Marshal.ReadInt16(text, i * 2) == ' ';

		switch (code)
		{
			case WbIsDelimiter:
				return current < length && IsSpace(current) ? 1 : 0;
			case WbClassify:
				return current < length && IsSpace(current) ? WbfIsWhite | WbfBreakLine : 0;
			case WbLeft:
			case WbMoveWordLeft:
			case WbLeftBreak:
			{
				int i = Math.Min(current, length) - 1;
				while (i > 0 && IsSpace(i))
					i--;
				while (i > 0 && !IsSpace(i - 1))
					i--;
				return Math.Max(i, 0);
			}
			default:
			{
				int i = current;
				while (i < length && !IsSpace(i))
					i++;
				while (i < length && IsSpace(i))
					i++;
				return i;
			}
		}
	}

	[LibraryImport("user32.dll")]
	private static partial IntPtr SendMessageW(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}

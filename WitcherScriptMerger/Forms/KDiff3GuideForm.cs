using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using WitcherScriptMerger.Controls;
using WitcherScriptMerger.Theming;
using WitcherScriptMerger.UI;

namespace WitcherScriptMerger.Forms;

/// <summary>
/// The KDiff3 guide's window (KDiff3Guide decides when it shows). First in front of KDiff3, centred on it, with what is
/// happening, how to read KDiff3, what to choose and where to get help - the longer parts folded. "Keep Open Beside
/// KDiff3" turns it into a narrow panel beside KDiff3's window, which moves to each next KDiff3 of the merge run.
/// Text is written as paragraphs: a leading "• " makes a bullet, and *stars* mark bold.
/// </summary>
internal sealed partial class KDiff3GuideForm : BaseForm
{
	private enum GuideMode { Full, Panel }

	// At 96 dpi.
	private static readonly Size FullSize = new(600, 600);
	internal const int PanelWidth = 380;
	private const int PanelMinHeight = 420;

	private readonly Font _titleFont = new("Segoe UI", 13F, FontStyle.Bold);
	private readonly Font _headerFont = new("Segoe UI", 11F, FontStyle.Bold);
	private readonly Font _textFont = new("Segoe UI", 10.5F);
	private readonly Font _boldFont = new("Segoe UI", 10.5F, FontStyle.Bold);

	private readonly FlowLayoutPanel _content;
	private readonly Label _title;
	private readonly RichTextBox _intro;
	private readonly RichTextBox _note;
	private readonly RichTextBox _easyWay;
	private readonly RichTextBox _reading;
	private readonly RichTextBox _choosing;
	private readonly RichTextBox _help;
	private readonly List<RichTextBox> _texts = [];
	private readonly CheckBox _option;
	private readonly RoundButton _keepOpen;
	private readonly RoundButton _close;

	private GuideMode _mode = GuideMode.Full;
	private bool _review;
	private KDiff3GuideContext _context;
	private IntPtr _kdiff3Window;
	private bool _seenSaved;

	/// <summary>Shown for a KDiff3 of the merge run, so the run's end closes it; false while opened from the menu only.</summary>
	internal bool ForKDiff3 { get; private set; }

	/// <param name="review">The review guide, remembered on its own, rather than the manual merging guide.</param>
	internal KDiff3GuideForm(bool review)
	{
		_review = review;
		// BaseForm scales by font; this form sizes itself in device units.
		AutoScaleMode = AutoScaleMode.None;
		Text = "KDiff3 Guide";
		Icon = Program.MainFrm.Icon;
		Font = new Font("Segoe UI", 10F);
		FormBorderStyle = FormBorderStyle.Sizable;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.Manual;
		MinimumSize = LogicalToDeviceUnits(new Size(300, 300));

		_content = new FlowLayoutPanel
		{
			AutoScroll = true,
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.TopDown,
			Margin = new Padding(0),
			Padding = Scaled(new Padding(20, 16, 12, 8)),
			WrapContents = false
		};
		_content.HandleCreated += (sender, e) => ThemeUtils.ApplyWindowTheme(_content);  // dark scrollbar in the dark theme
		_content.Resize += (sender, e) => FitTexts();

		_title = new Label { AutoSize = true, Font = _titleFont, Margin = Scaled(new Padding(0, 0, 0, 8)) };
		_content.Controls.Add(_title);

		_intro = AddText();
		_note = AddText();
		_note.Margin = Scaled(new Padding(0, 8, 0, 0));
		_easyWay = AddSection("The Easy Way Out", true);
		_reading = AddSection("How to Read KDiff3", true);
		_choosing = AddSection("What Should I Choose?", false);
		_help = AddSection("Need Help?", false);

		// Room below the last heading: scrolling this panel leaves out the bottom padding and margin, which hid most of a
		// folded last heading.
		_content.Controls.Add(new Panel { Height = LogicalToDeviceUnits(32), Margin = new Padding(0), Width = 1 });

		_option = new CheckBox { Anchor = AnchorStyles.Left, AutoSize = true, Margin = Scaled(new Padding(16, 0, 8, 0)), Text = "Show this guide every time" };
		_option.CheckedChanged += (sender, e) =>
		{
			if (_mode == GuideMode.Panel)
				TopMost = _option.Checked;
		};

		_keepOpen = CreateButton("Keep Open Beside KDiff3", new Padding(0, 0, 8, 0));
		_keepOpen.Click += (sender, e) => SwitchToPanel();

		_close = CreateButton("Close", new Padding(0, 0, 16, 0));
		_close.Click += (sender, e) => Close();
		CancelButton = _close;

		// The theme colours a TableLayoutPanel and its children, and scales the button row's height (designed at 125%).
		TableLayoutPanel buttons = new() { ColumnCount = 4, Dock = DockStyle.Fill, Margin = new Padding(0), RowCount = 1 };
		_ = buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		_ = buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
		_ = buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		_ = buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		_ = buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
		buttons.Controls.Add(_option, 0, 0);
		buttons.Controls.Add(_keepOpen, 2, 0);
		buttons.Controls.Add(_close, 3, 0);
		_keepOpen.Anchor = _close.Anchor = AnchorStyles.Right;

		TableLayoutPanel layout = new() { ColumnCount = 1, Dock = DockStyle.Fill, Margin = new Padding(0), RowCount = 2 };
		_ = layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
		_ = layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
		_ = layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
		layout.Controls.Add(_content, 0, 0);
		layout.Controls.Add(buttons, 0, 1);
		Controls.Add(layout);

		_option.Checked = KDiff3Guide.EveryTime(review);
	}

	#region Showing

	/// <summary>Opened from the menu, with no KDiff3: in general terms, centred on SM-FAE.</summary>
	internal void ShowCentredOn(Form owner)
	{
		ArgumentNullException.ThrowIfNull(owner);
		_keepOpen.Visible = false;
		Size = LogicalToDeviceUnits(FullSize);
		Location = Centre(owner.Bounds, Size, Screen.FromControl(owner).WorkingArea);
		Show(owner);
	}

	/// <summary>
	/// For this KDiff3 window and merge: first in front of it, centred and on top - KDiff3 is another program's window, and
	/// only "on top" reliably puts this one before it - or, as the panel, moved beside it without taking the focus.
	/// </summary>
	internal void AttachTo(IntPtr kdiff3Window, KDiff3GuideContext context)
	{
		ForKDiff3 = true;
		_kdiff3Window = kdiff3Window;
		_context = context;

		// Opened from the menu as the manual merging guide, and now for a review: it becomes the review guide.
		if (_mode == GuideMode.Full && !_seenSaved && context.IsReview != _review)
		{
			_review = context.IsReview;
			_option.Checked = KDiff3Guide.EveryTime(_review);
		}
		if (IsHandleCreated)
			WriteTexts();

		Rectangle kdiff3 = WindowBounds(kdiff3Window);
		Rectangle screen = Screen.FromHandle(kdiff3Window).WorkingArea;

		if (_mode == GuideMode.Panel)
		{
			Bounds = PanelBounds(kdiff3, screen);
			return;
		}

		_keepOpen.Visible = true;
		Size = LogicalToDeviceUnits(FullSize);
		Location = Centre(kdiff3, Size, screen);
		TopMost = true;
		if (!Visible)
			Show();
		Activate();
		_ = SetForegroundWindow(Handle);
	}

	/// <summary>The KDiff3 it was shown for has closed: shown in front of it, it goes too; the panel waits for the next.</summary>
	internal void KDiff3Gone()
	{
		_kdiff3Window = IntPtr.Zero;
		if (_mode == GuideMode.Full && ForKDiff3)
			Close();
	}

	private void SwitchToPanel()
	{
		SaveSeen();
		_mode = GuideMode.Panel;
		_keepOpen.Visible = false;
		_option.Text = "Keep on top";
		_option.Checked = true;
		TopMost = true;

		if (_kdiff3Window != IntPtr.Zero && IsWindow(_kdiff3Window))
		{
			Bounds = PanelBounds(WindowBounds(_kdiff3Window), Screen.FromHandle(_kdiff3Window).WorkingArea);
			_ = SetForegroundWindow(_kdiff3Window);  // back to work in KDiff3
		}
	}

	// Beside KDiff3 where the screen has room - right first, then left - otherwise over its right edge.
	private Rectangle PanelBounds(Rectangle kdiff3, Rectangle screen)
	{
		int width = LogicalToDeviceUnits(PanelWidth);
		int top = Math.Max(screen.Top, kdiff3.Top);
		int bottom = Math.Min(screen.Bottom, kdiff3.Bottom);
		if (bottom - top < LogicalToDeviceUnits(PanelMinHeight))
		{
			top = screen.Top;
			bottom = screen.Bottom;
		}

		int left = screen.Right - kdiff3.Right >= width ? kdiff3.Right
			: kdiff3.Left - screen.Left >= width ? kdiff3.Left - width
			: screen.Right - width;
		return new Rectangle(left, top, width, bottom - top);
	}

	private static Point Centre(Rectangle on, Size size, Rectangle screen)
	{
		int x = on.Left + ((on.Width - size.Width) / 2);
		int y = on.Top + ((on.Height - size.Height) / 2);
		x = Math.Max(screen.Left, Math.Min(x, screen.Right - size.Width));
		y = Math.Max(screen.Top, Math.Min(y, screen.Bottom - size.Height));
		return new Point(x, y);
	}

	// The window as seen: without the invisible resize border Windows 10 and later add around it.
	private static Rectangle WindowBounds(IntPtr window)
	{
		if (DwmGetWindowAttribute(window, DwmwaExtendedFrameBounds, out Rect rect, Marshal.SizeOf<Rect>()) != 0
			&& !GetWindowRect(window, out rect))
		{
			return Rectangle.Empty;
		}

		return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
	}

	// Leaving its first, full form - closed, or kept open as the panel - the guide counts as seen.
	private void SaveSeen()
	{
		if (_seenSaved)
			return;
		_seenSaved = true;
		KDiff3Guide.SaveSeen(_review, _option.Checked);
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (_mode == GuideMode.Full)
			SaveSeen();
		base.OnFormClosing(e);
	}

	// Written once the window exists - formatting a text box before it has one is fragile - with the theme in force.
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		WriteTexts();
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		FitTexts();
		ActiveControl = _close;
	}

	#endregion

	#region Text

	private void WriteTexts()
	{
		KDiff3GuideContext context = _context;
		KDiff3Reason reason = context?.Reason ?? KDiff3Reason.Conflict;
		bool threePanes = context?.HasGameFile ?? true;

		// The two sides by the names KDiff3 shows; from the menu, in general terms.
		string first = context == null ? "the first mod"
			: Bold(context.First) + (context.FirstIsMergeSoFar ? " (the mods merged so far)" : string.Empty);
		string second = context == null ? "the second mod" : Bold(context.Second);
		string file = context == null ? "the file" : Bold(context.RelativePath);
		string kind = context == null || context.RelativePath.EndsWithIgnoreCase(".ws") ? "script file" : "file";
		string letters = threePanes ? "*A*, *B* or *C*" : "*A* or *B*";
		string both = threePanes ? "both *B* and *C*" : "both *A* and *B*";

		_title.Text = context == null ? "Merging by Hand in KDiff3"
			: reason == KDiff3Reason.Review ? "Review This Merge in KDiff3"
			: reason == KDiff3Reason.UpdatedMod ? "Check This Merge in KDiff3"
			: "KDiff3 Needs Your Decision";

		Write(_intro, reason switch
		{
			KDiff3Reason.Review =>
				"KDiff3 is open because *Review each successful merge in KDiff3* is turned on in SM-FAE's options: every merge opens here for you to "
				+ $"check, even when SM-FAE could merge it on its own. This one is {first} and {second} in {file}. If lines are "
				+ "marked as a conflict, they need your decision either way.",
			KDiff3Reason.UpdatedMod =>
				$"{second} was updated since {file} was last merged. KDiff3 is open so you can check this merge: merging on its "
				+ $"own would keep changes the old version of {second} made, even ones the new version removed.",
			KDiff3Reason.NoGameFile =>
				$"{first} and {second} both change {file}, and the game has no original of it to compare with, so SM-FAE can't "
				+ "merge it on its own: every difference needs your decision.",
			_ when context == null =>
				"When two mods change some of the same lines in a script file, SM-FAE can't choose between them. It merges the "
				+ "rest of the file automatically and opens KDiff3 for you to decide those lines.",
			_ =>
				$"{first} and {second} change some of the same lines in the {kind} {file}. SM-FAE merged the rest of the file "
				+ "automatically; only these lines need your decision."
		});

		Write(_note, "Click a heading to open or close its section.");

		Write(_easyWay, reason switch
		{
			KDiff3Reason.Review =>
				"• If no lines are marked as a conflict, close KDiff3 and choose Save: the merge is kept as SM-FAE made it.\n"
				+ "• Turn it off in SM-FAE's options (Menu, Options...). SM-FAE then merges on its own, and "
				+ "opens KDiff3 only when two mods change the same lines.",
			KDiff3Reason.UpdatedMod =>
				"• Close KDiff3 without saving.\n"
				+ "• In SM-FAE, delete this merge and merge again: a new merge starts from the updated mod, so nothing of its "
				+ "old version is kept.",
			_ =>
				"Resolving script conflicts by hand can be difficult if you don't have experience reading script code. It's "
				+ "usually not something you learn in half an hour, starting from nothing. If you feel up to it, even with less "
				+ "experience, it's certainly not wrong to try: if the game doesn't start afterwards, Delete Merges in SM-FAE "
				+ "undoes everything you did, with no permanent harm.\n"
				+ "\n"
				+ "But if this looks like a bridge too far, there's an easy way out:\n"
				+ "• Close KDiff3 without saving. Nothing is merged for this file.\n"
				+ $"• Choose which of the two mods you want to keep, {first} or {second}, and uninstall the other.\n"
				+ "• Back in SM-FAE, refresh and merge again."
		});

		string panes = context == null ? "• The top panes: *A* is the game's own file, *B* and *C* are the two mods."
			: threePanes ? $"• The top panes: *A* is the game's own file, *B* is {first}, *C* is {second}."
			: $"• The top panes: *A* is {first}, *B* is {second}.";
		Write(_reading, panes + "\n"
			+ "• The bottom pane is the merged result that will be saved.\n"
			+ $"• Lines marked as a conflict need a decision. For each one, click {letters} in the toolbar to choose whose lines go into the result.\n"
			+ "• The toolbar's arrows jump to the next unsolved conflict.\n"
			+ "• After your first choice, the status bar at the bottom left shows how many conflicts are left. At 0, all are "
			+ "solved: close KDiff3, and choose Save when it asks.");

		Write(_choosing,
			"• Choosing one mod's lines drops the other mod's change to them, and that mod may then not work as intended.\n"
			+ $"• You can also choose {both}, which puts both mods' lines in the result. That only works if they don't "
			+ "contradict each other, and judging that takes reading the code.\n"
			+ "• Some mods can't be used together at all. Then the easy way out is the only way.");

		Write(_help,
			$"Ask in the Posts section of either mod's Nexus page. Say which mod it conflicts with, and name the file, {file}. "
			+ "The author or other users may know a fix, or have a compatibility patch.");
	}

	private static string Bold(string text) => "*" + text + "*";

	private RichTextBox AddText()
	{
		GuideText box = new()
		{
			BorderStyle = BorderStyle.None,
			BulletIndent = LogicalToDeviceUnits(14),
			Cursor = Cursors.Default,
			DetectUrls = false,
			Font = _textFont,
			Margin = new Padding(0),
			ReadOnly = true,
			ScrollBars = RichTextBoxScrollBars.None,
			TabStop = false,
			WordWrap = true
		};
		box.ContentsResized += (sender, e) => box.Height = e.NewRectangle.Height + LogicalToDeviceUnits(2);
		SpaceWordBreak.Apply(box);  // a path or mod name stays on one line
		_texts.Add(box);
		_content.Controls.Add(box);
		return box;
	}

	// A heading that folds its text open and closed.
	private RichTextBox AddSection(string heading, bool open)
	{
		Label header = new()
		{
			AutoSize = true,
			Cursor = Cursors.Hand,
			Font = _headerFont,
			Margin = Scaled(new Padding(0, 12, 0, 4)),
			Tag = "AccentLabel"
		};
		_content.Controls.Add(header);

		RichTextBox text = AddText();
		void SetOpen(bool show)
		{
			text.Visible = show;
			header.Text = (show ? "▾  " : "▸  ") + heading;
		}

		SetOpen(open);
		header.Click += (sender, e) =>
		{
			SetOpen(!text.Visible);
			FitTexts();
			_content.PerformLayout();
		};
		return text;
	}

	// Paragraphs on separate lines; "• " starts a bullet, *stars* mark bold. Coloured from the theme in force.
	private void Write(RichTextBox box, string text)
	{
		Color color = ThemeManager.CurrentTheme.ForeColor;
		box.Clear();

		string[] paragraphs = text.Split('\n');
		for (int p = 0; p < paragraphs.Length; p++)
		{
			string paragraph = paragraphs[p];
			bool bullet = paragraph.StartsWith("• ", StringComparison.Ordinal);
			if (bullet)
				paragraph = paragraph[2..];

			box.SelectionStart = box.TextLength;
			box.SelectionLength = 0;
			box.SelectionBullet = bullet;
			box.SelectionIndent = bullet ? LogicalToDeviceUnits(4) : 0;

			string[] parts = paragraph.Split('*');
			for (int i = 0; i < parts.Length; i++)
			{
				box.SelectionStart = box.TextLength;
				box.SelectionFont = i % 2 == 1 ? _boldFont : _textFont;
				box.SelectionColor = color;
				box.AppendText(parts[i]);
			}

			if (p < paragraphs.Length - 1)
				box.AppendText("\n");
		}

		RequestFit(box);
	}

	// Each text is as wide as the content allows, and as tall as its text.
	private void FitTexts()
	{
		int width = _content.ClientSize.Width - _content.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth;
		if (width <= 0)
			return;

		_title.MaximumSize = new Size(width, 0);
		foreach (RichTextBox box in _texts)
		{
			box.Width = width;
			RequestFit(box);
		}
	}

	// Asks the text box to report the height its text needs (ContentsResized), even when its width did not change it.
	private static void RequestFit(RichTextBox box)
	{
		if (box.IsHandleCreated)
			_ = SendMessageW(box.Handle, EmRequestResize, IntPtr.Zero, IntPtr.Zero);
	}

	#endregion

	private Padding Scaled(Padding padding) => new(
		LogicalToDeviceUnits(padding.Left), LogicalToDeviceUnits(padding.Top),
		LogicalToDeviceUnits(padding.Right), LogicalToDeviceUnits(padding.Bottom));

	private RoundButton CreateButton(string text, Padding margin)
	{
		// Rounded by the theme at its size then, so it is given its full width now - as a minimum too, or the auto-sized
		// button shrinks to its text when it becomes the default button.
		Font font = new("Segoe UI", 11F);
		int width = Math.Max(160, TextRenderer.MeasureText(text, font).Width + LogicalToDeviceUnits(40));
		return new RoundButton { AutoSize = true, Font = font, Margin = margin, MinimumSize = new Size(width, 50), Size = new Size(width, 50), Text = text };
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			// Disposed with the form's controls too; named here for the analyzer.
			_content.Dispose();
			_title.Dispose();
			_intro.Dispose();
			_reading.Dispose();
			_choosing.Dispose();
			_help.Dispose();
			_note.Dispose();
			_easyWay.Dispose();
			_option.Dispose();
			_keepOpen.Dispose();
			_close.Dispose();
			_titleFont.Dispose();
			_headerFont.Dispose();
			_textFont.Dispose();
			_boldFont.Dispose();
		}

		base.Dispose(disposing);
	}

	// A text that never scrolls by itself: the mouse wheel over it scrolls the guide, as anywhere else in it.
	private sealed class GuideText : RichTextBox
	{
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WmMouseWheel && Parent != null)
			{
				_ = SendMessageW(Parent.Handle, m.Msg, m.WParam, m.LParam);
				return;
			}

			base.WndProc(ref m);
		}
	}

	#region Native

	private const int WmMouseWheel = 0x020A;

	private const int DwmwaExtendedFrameBounds = 9;
	private const int EmRequestResize = 0x0400 + 65;

	[StructLayout(LayoutKind.Sequential)]
	private struct Rect
	{
		internal int Left, Top, Right, Bottom;
	}

	[LibraryImport("dwmapi.dll")]
	private static partial int DwmGetWindowAttribute(IntPtr hwnd, int attribute, out Rect value, int size);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetWindowRect(IntPtr hWnd, out Rect rect);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool IsWindow(IntPtr hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetForegroundWindow(IntPtr hWnd);

	[LibraryImport("user32.dll")]
	private static partial IntPtr SendMessageW(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	#endregion
}

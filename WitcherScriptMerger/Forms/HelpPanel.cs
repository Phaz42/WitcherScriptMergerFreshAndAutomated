using System;
using System.Drawing;
using System.Windows.Forms;

using WitcherScriptMerger.Controls;
using WitcherScriptMerger.Theming;
using WitcherScriptMerger.UI;

namespace WitcherScriptMerger.Forms;

/// <summary>
/// A help topic (HelpTopics), opened by a "?" (HelpButton): a large panel (OverlayPanel) with the topic's title and its
/// formatted text, which scrolls. A click on the "?" of the topic already open closes it.
/// </summary>
internal sealed class HelpPanel : OverlayPanel
{
	private readonly HelpTopic _topic;
	private readonly RichTextBox _text;
	private readonly Font _textFont = new("Segoe UI", 10.5F);
	private readonly Font _headingFont = new("Segoe UI", 12F, FontStyle.Bold);
	private readonly Font _exampleFont = new("Consolas", 10.5F);

	/// <summary>Opens the topic over the owner, closing any other panel first.</summary>
	internal static void ShowTopic(Form owner, HelpTopic topic)
	{
		ArgumentNullException.ThrowIfNull(topic);
		Open(owner, "help:" + topic.Title, () => new HelpPanel(owner, topic));
	}

	private HelpPanel(Form owner, HelpTopic topic) : base(owner, "help:" + topic.Title, topic.Title, 0.6, 560)
	{
		_topic = topic;
		Theme theme = ThemeManager.CurrentTheme;

		_text = new RichTextBox
		{
			BackColor = theme.TreeViewBackColor,
			BorderStyle = BorderStyle.None,
			DetectUrls = false,
			Dock = DockStyle.Fill,
			Font = _textFont,
			ForeColor = theme.ForeColor,
			ReadOnly = true,
			ScrollBars = RichTextBoxScrollBars.Vertical,
			TabStop = false,
			WordWrap = true
		};
		_text.HandleCreated += (sender, e) => ThemeUtils.ApplyWindowTheme(_text);  // dark scrollbar in the dark theme
		SpaceWordBreak.Apply(_text);  // a path or a name stays on one line
		_text.BulletIndent = owner.LogicalToDeviceUnits(16);  // from a bullet to its text

		Body.Controls.Add(_text);
	}

	// Filled once its handle exists; each run's color and font set as it is written, as in the problems report.
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		Theme theme = ThemeManager.CurrentTheme;
		int indent = LogicalToDeviceUnits(22);
		bool first = true;
		foreach (HelpLine line in _topic.Lines)
		{
			// A new paragraph takes the previous one's format: it is set for each line, and a heading's blank line before
			// it is a plain paragraph, not a bullet.
			if (!first)
			{
				Append(Environment.NewLine, _textFont, theme.ForeColor);
				if (line.Style == HelpStyle.Heading)
				{
					SetParagraph(HelpStyle.Text, indent);
					Append(Environment.NewLine, _textFont, theme.ForeColor);
				}
			}

			first = false;
			SetParagraph(line.Style, indent);

			switch (line.Style)
			{
				case HelpStyle.Heading:
					Append(line.Text, _headingFont, theme.AccentColor);
					break;
				case HelpStyle.Example:
					Append(line.Example, _exampleFont, theme.ForeBrightColor);
					if (!string.IsNullOrEmpty(line.Text))
						Append("   " + line.Text, _textFont, theme.ForeDimmedColor);
					break;
				default:
					Append(line.Text, _textFont, theme.ForeColor);
					break;
			}
		}

		_text.SelectionStart = 0;
		_text.SelectionLength = 0;
	}

	// Examples and bullets indented alike; a bullet's text, wrapped lines included, starts after its bullet.
	private void SetParagraph(HelpStyle style, int indent)
	{
		_text.SelectionStart = _text.TextLength;
		_text.SelectionLength = 0;
		_text.SelectionBullet = style == HelpStyle.Bullet;
		_text.SelectionIndent = style is HelpStyle.Example or HelpStyle.Bullet ? indent : 0;
	}

	private void Append(string text, Font font, Color color)
	{
		_text.SelectionStart = _text.TextLength;
		_text.SelectionLength = 0;
		_text.SelectionFont = font;
		_text.SelectionColor = color;
		_text.AppendText(text);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_text.Dispose();
			_textFont.Dispose();
			_headingFont.Dispose();
			_exampleFont.Dispose();
		}

		base.Dispose(disposing);
	}
}

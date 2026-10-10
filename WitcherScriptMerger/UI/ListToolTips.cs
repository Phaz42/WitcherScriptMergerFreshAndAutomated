using System;
using System.Drawing;
using System.Windows.Forms;

namespace WitcherScriptMerger.UI;

/// <summary>
/// A list's tooltips, shown by SM-FAE rather than by the list. Windows' tree view counts its tooltip delay from when the
/// mouse enters the list, not the row, so after the first tooltip every row's appeared at once. Here each one waits the
/// delay on its own row, and while one is showing the next follows at once, as Windows' tooltips do elsewhere.
/// A row's tooltip is its node's ToolTipText, over the node's text; the conflicts list adds its load order numbers.
/// </summary>
internal sealed class ListToolTips : IDisposable
{
	private readonly TreeView _tree;
	private readonly ToolTip _toolTip;
	private readonly Func<TreeNode, Point, string> _numberToolTip;
	private readonly Timer _timer;

	// What the mouse is on: a node's load order number, or its text. Null: nothing with a tooltip.
	private TreeNode _node;
	private bool _onNumber;
	private bool _shown;

	/// <param name="numberToolTip">The tooltip of the load order number at a point of a node's row, or null.</param>
	internal ListToolTips(TreeView tree, ToolTip toolTip, Func<TreeNode, Point, string> numberToolTip = null)
	{
		_tree = tree;
		_toolTip = toolTip;
		_numberToolTip = numberToolTip;

		_timer = new Timer { Interval = toolTip.InitialDelay };
		_timer.Tick += (sender, e) =>
		{
			_timer.Stop();
			// Shown only if the mouse is still on it.
			Point mouse = _tree.PointToClient(Cursor.Position);
			(TreeNode node, bool onNumber, string text) = Find(mouse);
			if (text != null && node == _node && onNumber == _onNumber)
				Show(text, node, mouse);
		};

		// No mouse move follows these to hide it.
		_tree.MouseLeave += (sender, e) => Hide();
		_tree.MouseDown += (sender, e) => Hide();
		_tree.MouseWheel += (sender, e) => Hide();
	}

	internal void OnMouseMove(Point location)
	{
		(TreeNode node, bool onNumber, string text) = Find(location);
		if (text == null)
		{
			Hide();
			return;
		}

		if (node == _node && onNumber == _onNumber)
			return;

		_node = node;
		_onNumber = onNumber;
		_timer.Stop();
		if (_shown)
			Show(text, node, location);
		else
			_timer.Start();
	}

	private (TreeNode Node, bool OnNumber, string Text) Find(Point location)
	{
		TreeNode node = _tree.GetNodeAt(location);
		if (node == null)
			return (null, false, null);

		string number = _numberToolTip?.Invoke(node, location);
		if (number != null)
			return (node, true, number);

		return node.Bounds.Contains(location) && !string.IsNullOrEmpty(node.ToolTipText)
			? (node, false, node.ToolTipText)
			: (null, false, null);
	}

	// Shown twice: WinForms' ToolTip.Show doesn't appear the first time after a visible tooltip was hidden, nor the very
	// first time - moving from one group's load order number to the next group's showed nothing. A second call shows it.
	// Each Show adds a handler to the form's Deactivate event that only a Hide removes, so every Show is matched by a Hide
	// (HideToolTip); otherwise the handlers pile up as the mouse crosses rows. An extra Hide does nothing.
	// WinForms shows nothing while another window is active, so then the tooltip doesn't count as shown: the next row
	// waits the delay, as the first does.
	private void Show(string text, TreeNode node, Point mouse)
	{
		HideToolTip();
		if (_tree.FindForm() is not { } form || form != Form.ActiveForm)
			return;

		Point location = new(mouse.X + 16, node.Bounds.Top + 3);
		_toolTip.Show(text, _tree, location);
		_toolTip.Show(text, _tree, location);
		_shown = true;
	}

	/// <summary>Hides the tooltip; also when the list is rebuilt under a resting mouse, which no mouse event reports.</summary>
	internal void Hide()
	{
		_timer.Stop();
		HideToolTip();
		_node = null;
		_onNumber = false;
	}

	private void HideToolTip()
	{
		if (_shown)
		{
			_toolTip.Hide(_tree);
			_toolTip.Hide(_tree);
		}
		_shown = false;
	}

	public void Dispose() => _timer.Dispose();
}

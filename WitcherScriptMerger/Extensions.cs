using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WitcherScriptMerger;

internal static partial class Extensions
{
	#region Strings

#pragma warning disable CA1307 // Specify StringComparison for clarity
	internal static string ReplaceIgnoreCase(this string s, string oldValue, string newValue) => Regex.Replace(s, Regex.Escape(oldValue), newValue.Replace("$", "$$"), RegexOptions.IgnoreCase);
#pragma warning restore CA1307 // Specify StringComparison for clarity

#pragma warning disable CA1309 // Use ordinal string comparison
	internal static bool EqualsIgnoreCase(this string s, string otherString) => s.Equals(otherString, StringComparison.InvariantCultureIgnoreCase);
#pragma warning restore CA1309 // Use ordinal string comparison

	internal static int IndexOfIgnoreCase(this string s, string value, int startIndex = 0) => s.IndexOf(value, startIndex, StringComparison.InvariantCultureIgnoreCase);

	internal static int LastIndexOfIgnoreCase(this string s, string value, int startIndex = -1)
	{
		if (startIndex == -1)
			startIndex = s.Length - 1;
		return s.LastIndexOf(value, startIndex, StringComparison.InvariantCultureIgnoreCase);
	}

	internal static bool StartsWithIgnoreCase(this string s, string value) => s.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);

	internal static bool EndsWithIgnoreCase(this string s, string value) => s.EndsWith(value, StringComparison.InvariantCultureIgnoreCase);

	internal static bool IsAlphaNumeric(this string s) => new Regex("^[_a-zA-Z0-9]*$").IsMatch(s);

	internal static string GetPluralS(this int num) => num == 1 ? "" : "s";

	#endregion

	#region FileInfo
	internal static string ResolveTargetFileFullName(this FileInfo fileInfo) => fileInfo.Exists ? SimLink.GetSymbolicLinkTarget(fileInfo) : fileInfo.FullName;
	#endregion

	#region Tree & Context Menu

	internal static IEnumerable<ToolStripItem> GetAvailableItems(this ContextMenuStrip menu) => menu.Items.Cast<ToolStripItem>().Where(item => item.Available);

	internal static void SetFontStyle(this TreeNode node, FontStyle style)
	{
		Font currFont = node.NodeFont ?? Control.DefaultFont;
		node.NodeFont = new Font(currFont, style);
	}

	internal static IEnumerable<TreeNode> GetTreeNodes(this TreeNode node) => node.Nodes.Cast<TreeNode>();

	internal static Controls.SMTree.NodeMetadata GetMetadata(this TreeNode node) => node.Tag as Controls.SMTree.NodeMetadata;

	internal static bool IsEmpty(this TreeView tree) => tree.Nodes.Count == 0;

	#endregion

	#region Scrolling TreeView to Top

	private const int WM_VSCROLL = 0x0115;
	private const int SB_THUMBPOSITION = 0x0004;

	[LibraryImport("user32.dll")]
	private static partial int SetScrollPos(IntPtr hWnd, int nBar, int nPos, [MarshalAs(UnmanagedType.Bool)] bool bRedraw);

	[LibraryImport("user32.dll", EntryPoint = "PostMessageA")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

	internal static void ScrollToTop(this TreeView treeView)
	{
		if (SetScrollPos(treeView.Handle, WM_VSCROLL, 0, true) != -1)
			_ = PostMessage(treeView.Handle, WM_VSCROLL, SB_THUMBPOSITION, 0);
	}

	#endregion

	#region TreeView Checkbox Visibility

	// From http://stackoverflow.com/a/22488652/1641069

	private const int TVIF_STATE = 0x8;
	private const int TVIS_STATEIMAGEMASK = 0xF000;
	private const int TV_FIRST = 0x1100;
	private const int TVM_GETITEM = TV_FIRST + 62;
	private const int TVM_SETITEM = TV_FIRST + 63;

	[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
	private struct TVITEM
	{
		internal int mask;
		internal IntPtr hItem;
		internal int state;
		internal int stateMask;
		[MarshalAs(UnmanagedType.LPTStr)]
		internal string lpszText;
		internal int cchTextMax;
		internal int iImage;
		internal int iSelectedImage;
		internal int cChildren;
		internal IntPtr lParam;
	}

	[DllImport("user32.dll")]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
	private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref TVITEM lParam);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

	/// <summary>
	/// Gets a value indicating if the checkbox is visible on the tree node.
	/// </summary>
	/// <param name="node">The tree node.</param>
	/// <returns><value>true</value> if the checkbox is visible on the tree node; otherwise <value>false</value>.</returns>
	internal static bool IsCheckBoxVisible(this TreeNode node)
	{
		ArgumentNullException.ThrowIfNull(node);
		if (node.TreeView == null)
			throw new InvalidOperationException("The node does not belong to a tree.");
		TVITEM tvi = new()
		{
			hItem = node.Handle,
			mask = TVIF_STATE
		};
		IntPtr result = SendMessage(node.TreeView.Handle, TVM_GETITEM, node.Handle, ref tvi);
		if (result == IntPtr.Zero)
			throw new InvalidOperationException("Error getting TreeNode state.");
		int imageIndex = (tvi.state & TVIS_STATEIMAGEMASK) >> 12;
		return imageIndex != 0;
	}

	/// <summary>
	/// Sets a value indicating if the checkbox is visible on the tree node.
	/// </summary>
	/// <param name="node">The tree node.</param>
	/// <param name="isVisible"><value>true</value> to make the checkbox visible on the tree node; otherwise <value>false</value>.</param>
	internal static void SetIsCheckBoxVisible(this TreeNode node, bool isVisible, bool applyToSubtree = false)
	{
		if (node.TreeView == null)
			throw new InvalidOperationException("The node does not belong to a tree.");
		TVITEM tvi = new()
		{
			hItem = node.Handle,
			mask = TVIF_STATE,
			stateMask = TVIS_STATEIMAGEMASK,
			state = (isVisible ? node.Checked ? 2 : 1 : 0) << 12
		};
		IntPtr result = SendMessage(node.TreeView.Handle, TVM_SETITEM, IntPtr.Zero, ref tvi);
		if (result == IntPtr.Zero)
			throw new InvalidOperationException("Error setting TreeNode state.");

		if (applyToSubtree)
		{
			foreach (TreeNode childNode in node.GetTreeNodes())
			{
				childNode.SetIsCheckBoxVisible(isVisible, applyToSubtree);
			}
		}
	}

	internal static bool SetCheckedIfVisible(this TreeNode node, bool isChecked)
	{
		if (node.IsCheckBoxVisible())
		{
			node.Checked = isChecked;
			return true;
		}

		return false;
	}

	internal static bool AreAllVisibleCheckboxesChecked(this TreeNode node)
	{
		return node.GetTreeNodes()
			.Where(child => child.IsCheckBoxVisible())
			.All(child => child.Checked);
	}

	#endregion
}

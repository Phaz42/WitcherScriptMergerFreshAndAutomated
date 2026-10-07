using System;
using System.Collections;
using System.Windows.Forms;

using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.LoadOrder;

namespace WitcherScriptMerger.Controls;

internal class SMTreeSorter : IComparer
{
	public int Compare(object x, object y)
	{
		TreeNode xNode = x as TreeNode;
		TreeNode yNode = y as TreeNode;

		switch (xNode.Level)
		{
			case (int)SMTree.LevelType.Categories:
				ModFileCategory xCat = (ModFileCategory)xNode.Tag;
				ModFileCategory yCat = (ModFileCategory)yNode.Tag;
				return xCat.OrderIndex.CompareTo(yCat.OrderIndex);
			case (int)SMTree.LevelType.Mods:
				// A non-mergeable conflict is decided by the load order alone, so its mods are listed in that order, the
				// winner first. Mergeable ones stay in name order: a merge decides them, not the load order, and they are
				// merged in the order listed. (A node being added has no parent yet; the other one does.)
				if ((xNode.Parent ?? yNode.Parent)?.Parent?.Tag is ModFileCategory { IsSupported: false } && Program.LoadOrder != null)
					return Program.LoadOrder.CompareByLoadOrder(xNode.Text, yNode.Text);
				return new LoadOrderComparer().Compare(xNode.Text, yNode.Text);
			default:
				return string.Compare(xNode.Text, yNode.Text, StringComparison.Ordinal);
		}
	}
}
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
				return new LoadOrderComparer().Compare(xNode.Text, yNode.Text);
			default:
				return string.Compare(xNode.Text, yNode.Text, StringComparison.Ordinal);
		}
	}
}
using System.Linq;
using System.Windows.Forms;

namespace WitcherScriptMerger.Controls;

internal class ToolStripRegion(ToolStrip owner, ToolStripItem[] value)
{
	internal ToolStripItemCollection Items = new(owner, value);

	internal bool Available => Items.Cast<ToolStripItem>().Any(item => item.Available);
}

using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using WitcherScriptMerger.FileIndex;

namespace WitcherScriptMerger.Inventory;

[XmlRoot]
public class Merge : ModFile
{
	[XmlElement]
	public string MergedModName { get; set; }

	internal string GetMergedFile()
	{
		return Category == Categories.Script
			? Path.Combine(Paths.ModsDirectory, MergedModName, Paths.ModScriptBase, RelativePath)
			: Category == Categories.Xml
			? Path.Combine(Paths.ModsDirectory, MergedModName, RelativePath)
			: Category == Categories.BundleText
			? Path.Combine(Paths.MergedBundleContent, RelativePath)
			: throw new NotImplementedException();
	}

	internal string GetMergedBundle()
	{
		return Category != Categories.BundleText
			? throw new InvalidOperationException($"Can't get bundle for file of category '{Category.DisplayName}'.")
			: Path.Combine(Paths.ModsDirectory, MergedModName, Paths.BundleBase, BundleName);
	}

	internal FileHash GetHashByModName(string modName) => Mods.FirstOrDefault(m => m.Name.EqualsIgnoreCase(modName));
}

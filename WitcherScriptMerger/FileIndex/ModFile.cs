using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using WitcherScriptMerger.Inventory;

namespace WitcherScriptMerger.FileIndex;

public class ModFile
{
	#region Members

	[XmlElement]
	public string RelativePath { get; set; }

	[XmlElement("IncludedMod")]
	public Collection<FileHash> Mods { get; private set; }

	[XmlElement]
	public string BundleName { get; set; }

	[XmlIgnore]
	public ModFileCategory Category
	{
		get
		{
			return BundleName != null
				? IsTextFile(RelativePath) ? Categories.BundleText : Categories.BundleNotMergeable
				: IsScript(RelativePath) ? Categories.Script : IsXml(RelativePath) ? Categories.Xml : Categories.FlatNotMergeable;
		}
	}

	[XmlIgnore]
	public bool IsBundleContent => BundleName != null;

	[XmlIgnore]
	public bool HasConflict => Mods.Count > 1;

	#endregion

	internal ModFile(string relPath, string bundlePath = null)
	{
		RelativePath = relPath;
		Mods = [];
		if (bundlePath != null)
			BundleName = Path.GetFileName(bundlePath);
	}

	internal ModFile() => Mods = [];

	internal bool ContainsMod(string modName) => Mods.Any(mod => mod.Name.EqualsIgnoreCase(modName));

	internal string GetVanillaFile()
	{
		return Category == Categories.Script
			? Path.Combine(Paths.ScriptsDirectory, RelativePath)
			: Category == Categories.Xml
			? Path.Combine(Paths.GameDirectory, RelativePath)
			: throw new InvalidOperationException($"Can't get vanilla file for category '{Category.DisplayName}'.");
	}

	internal string GetModFile(string modName)
	{
		return Category == Categories.Script
			? Path.Combine(Paths.ModsDirectory, modName, Paths.ModScriptBase, RelativePath)
			: Category == Categories.Xml
			? Path.Combine(Paths.ModsDirectory, modName, RelativePath)
			: Category.IsBundled
			? Path.Combine(Paths.ModsDirectory, modName, Paths.BundleBase, BundleName)
			: throw new NotImplementedException();
	}

	internal static string GetModNameFromPath(string modFilePath)
	{
		if (modFilePath == null)
			throw new ArgumentNullException(nameof(modFilePath));
		if (!modFilePath.StartsWithIgnoreCase(Paths.ModsDirectory))  // Merged bundle content has internal path, not derived from mod folder
			return Paths.MergedBundleContent;

		int nameStart = Paths.ModsDirectory.Length + 1;
		string name = modFilePath[nameStart..];
		return name[..name.IndexOf('\\', StringComparison.OrdinalIgnoreCase)];
	}

	internal static bool IsScript(string path) => path == null ? throw new ArgumentNullException(nameof(path)) : path.EndsWithIgnoreCase(".ws");

	internal static bool IsXml(string path) => path == null ? throw new ArgumentNullException(nameof(path)) : path.EndsWithIgnoreCase(".xml");

	internal static bool IsFlatFile(string path) => IsScript(path) || IsXml(path);

	internal static bool IsBundle(string path) => path == null ? throw new ArgumentNullException(nameof(path)) : path.EndsWithIgnoreCase(".bundle");

	internal static bool IsTextFile(string path) => path == null ? throw new ArgumentNullException(nameof(path)) : path.EndsWithIgnoreCase(".ws") || path.EndsWithIgnoreCase(".xml") || path.EndsWithIgnoreCase(".txt") || path.EndsWithIgnoreCase(".csv");

	public override string ToString() => $"({Mods.Count} mod{Mods.Count.GetPluralS()}) {RelativePath}";
}
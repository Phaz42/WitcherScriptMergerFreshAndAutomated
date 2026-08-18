namespace WitcherScriptMerger.FileIndex;

public class ModFileCategory(int orderIndex, string displayName, string toolTipText, bool isSupported, bool isBundled)
{
	internal int OrderIndex { get; private set; } = orderIndex;
	internal string DisplayName { get; private set; } = displayName;
	internal string ToolTipText { get; private set; } = toolTipText;
	internal bool IsSupported { get; private set; } = isSupported;
	internal bool IsBundled { get; private set; } = isBundled;

	public override string ToString() => DisplayName;
}

internal static class Categories
{
	internal static ModFileCategory Script = new(
		1, "Scripts", "These plaintext .ws files can be merged", true, false);

	internal static ModFileCategory Xml = new(
		2, "Non-Bundled XML", "These .xml text files can be merged", true, false);

	internal static ModFileCategory BundleText = new(
		3, "Bundled Text", "These bundled text files can be merged", true, true);

	internal static ModFileCategory BundleNotMergeable = new(
		4, "Bundled Non-text - Not Mergeable", "Right-click mods to define your load order instead of merging", false, true);

	internal static ModFileCategory FlatNotMergeable = new(
		5, "Not Mergeable", "Script Merger doesn't know what these files are", false, false);
}

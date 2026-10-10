namespace WitcherScriptMerger.FileIndex;

/// <param name="toolTipText">The section's tooltip in the conflicts list.</param>
/// <param name="mergesToolTipText">Its tooltip in the merges list; null for a section that is never merged.</param>
public class ModFileCategory(int orderIndex, string displayName, string toolTipText, string mergesToolTipText, bool isSupported, bool isBundled)
{
	internal int OrderIndex { get; private set; } = orderIndex;
	internal string DisplayName { get; private set; } = displayName;
	internal string ToolTipText { get; private set; } = toolTipText;
	internal string MergesToolTipText { get; private set; } = mergesToolTipText;
	internal bool IsSupported { get; private set; } = isSupported;
	internal bool IsBundled { get; private set; } = isBundled;

	public override string ToString() => DisplayName;
}

internal static class Categories
{
	internal static ModFileCategory Script = new(
		1, "Scripts",
		"Script files (.ws) changed by more than one mod. SM-FAE can merge them into one file that keeps every mod's changes.",
		"Script files SM-FAE has merged. Each one combines the changes of the mods listed under it.",
		true, false);

	internal static ModFileCategory Xml = new(
		2, "Non-Bundled XML",
		"Loose XML files changed by more than one mod. SM-FAE can merge them into one file that keeps every mod's changes.",
		"XML files SM-FAE has merged. Each one combines the changes of the mods listed under it.",
		true, false);

	internal static ModFileCategory BundleText = new(
		3, "Bundled Text",
		"Text files inside mods' bundles, changed by more than one mod. SM-FAE can merge them and pack the result into a bundle of its own.",
		"Bundled text files SM-FAE has merged and packed into the Merged Mod's own bundle.",
		true, true);

	internal static ModFileCategory BundleNotMergeable = new(
		4, "Bundled Non-Text - Not Mergeable",
		"Packed files that can't be merged, such as textures, models and menus. The game uses one mod's copy: the one with the lowest load order number, listed first. Double-click a mod to change its number.",
		null,
		false, true);

	internal static ModFileCategory FlatNotMergeable = new(
		5, "Not Mergeable",
		"Loose files SM-FAE can't merge, because it doesn't know their format. The game uses one mod's copy, chosen by the load order.",
		null,
		false, false);
}

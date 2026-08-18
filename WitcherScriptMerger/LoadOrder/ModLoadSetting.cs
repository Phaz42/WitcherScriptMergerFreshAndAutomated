namespace WitcherScriptMerger.LoadOrder;

internal class ModLoadSetting
{
	internal string ModName { get; set; }

	internal string VK { get; set; }

	internal bool? IsEnabled { get; set; }

	internal int? Priority { get; set; }

	internal ModLoadSetting()
	{ }

	internal ModLoadSetting(string modName) => ModName = modName;

	public override string ToString() => $"{ModName}, priority {Priority}, {(!IsEnabled.HasValue || IsEnabled.Value ? "enabled" : "disabled")}";
}

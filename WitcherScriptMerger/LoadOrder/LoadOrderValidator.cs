using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WitcherScriptMerger.LoadOrder;

internal static class LoadOrderValidator
{
	internal static void ValidateAndFix(CustomLoadOrder loadOrder)
	{
		if (!loadOrder.Mods.Any())
			return;

		string mergedModName = Paths.RetrieveMergedModName();
		ModLoadSetting mergedMod = loadOrder.Mods.FirstOrDefault(m => m.ModName.EqualsIgnoreCase(mergedModName));

		if (mergedMod != null && mergedMod == loadOrder.GetTopPriorityEnabledMod())
			return;

		DialogResult choice = PromptToPrioritizeMergedMod(loadOrder.FilePath);
		if (choice == DialogResult.Yes)
		{
			PrioritizeMergedMod(loadOrder, mergedMod);
		}
		else if (choice == DialogResult.Cancel)  // Never
		{
			Program.Settings.Set("ValidateCustomLoadOrder", false);
			Program.Settings.Save();
		}
	}

	internal static List<KeyValuePair<int, string>> CheckForDuplicatePrios(CustomLoadOrder loadOrder)
	{
		List<KeyValuePair<int, string>> duplicates = [];
		Dictionary<int, List<string>> priorities = [];

		// Group ModLoadSetting objects by Priority
		foreach (ModLoadSetting mod in loadOrder.Mods)
		{
			if (mod.Priority.HasValue)
			{
				if (!priorities.ContainsKey(mod.Priority.Value))
				{
					priorities[mod.Priority.Value] = [];
				}

				priorities[mod.Priority.Value].Add(mod.ModName);
			}
		}

		// Look for duplicates and add them to the result list
		foreach (KeyValuePair<int, List<string>> priority in priorities)
		{
			if (priority.Value.Count > 1)
			{
				foreach (string modName in priority.Value)
				{
					duplicates.Add(new KeyValuePair<int, string>(priority.Key, modName));
				}
			}
		}

		return duplicates;
	}

	private static DialogResult PromptToPrioritizeMergedMod(string modsSettingsPath)
	{
		DialogResult choice = MessageBox.Show(
			$"SM-FAE found a custom load order in {modsSettingsPath}, but the Merged Mod isn't set to load first.\n\n" +
			"Should SM-FAE change the load order so that the Merged Mod loads first?\n\n" +
			"Yes:       Recommended\nNo:        Not recommended, the merged scripts may not get loaded\nCancel:  No, and never perform this check again",
			"Custom Load Order Problem",
			MessageBoxButtons.YesNoCancel,
			MessageBoxIcon.Exclamation,
			MessageBoxDefaultButton.Button1);

		return choice;
	}

	private static void PrioritizeMergedMod(CustomLoadOrder loadOrder, ModLoadSetting mergedModSetting)
	{
		int priority = CustomLoadOrder.TopPriority;

		if (mergedModSetting != null)
		{
			// Through the load order's own actions, which reread the file, move a mod already at that number down, and save
			loadOrder.SetPriorityByName(mergedModSetting.ModName, priority);
			if (loadOrder.IsModDisabledByName(mergedModSetting.ModName))
				loadOrder.ToggleModByName(mergedModSetting.ModName);
		}
		else
		{
			loadOrder.Refresh();
			if (!loadOrder.IsValid)
				return;

			loadOrder.AddMod(new ModLoadSetting
			{
				ModName = Paths.RetrieveMergedModName(),
				IsEnabled = true,
				Priority = priority,
				VK = Paths.RetrieveMergedModName()
			});
		}
	}
}

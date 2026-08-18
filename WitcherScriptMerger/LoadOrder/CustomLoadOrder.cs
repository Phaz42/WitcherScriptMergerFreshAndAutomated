using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.LoadOrder;

internal partial class CustomLoadOrder : IDisposable
{
	internal const int TopPriority = 0;
	internal const int BottomPriority = 9999;
	private bool disposedValue;
	private readonly FileSystemWatcher LoadOrderWatcher = new();
	private Timer LoadOrderTimer;

	private List<ModLoadSetting> _mods = [];
	internal IReadOnlyList<ModLoadSetting> Mods
	{
		get => _mods;
		private set => _mods = value.OrderBy(m => m.Priority).ThenBy(m => m.ModName).ToList();
	}

	private static readonly string LoadOrderDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "The Witcher 3");
	private const string LoadOrderFileName = "mods.settings";
	internal readonly string FilePath = Path.Combine(LoadOrderDirectory, LoadOrderFileName);


	internal bool IsValid { get; private set; }

	internal CustomLoadOrder()
	{
		Refresh();
		SetupLoadOrderWatcher();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// Dispose managed state (managed objects)
				LoadOrderWatcher.Dispose();
				LoadOrderTimer.Dispose();
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#region File Processing

	private void SetupLoadOrderWatcher()
	{
		LoadOrderWatcher.Path = LoadOrderDirectory;
		LoadOrderWatcher.Filter = LoadOrderFileName;
		LoadOrderWatcher.Changed += LoadOrderWatcher_Changed;
		LoadOrderWatcher.EnableRaisingEvents = true;

		LoadOrderTimer = new Timer(1000) { AutoReset = false };
		LoadOrderTimer.Elapsed += LoadOrderTimer_Elapsed;
	}

	private void LoadOrderTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		Refresh();
		_ = MainFrm.treConflicts.BeginInvoke(MainFrm.treConflicts.SetStylesForCustomLoadOrder);
	}

	private void LoadOrderWatcher_Changed(object sender, FileSystemEventArgs e)
	{
		LoadOrderTimer.Stop();
		LoadOrderTimer.Start();
	}

	internal void Refresh(bool keepVortexKey = false)
	{
		IsValid = false;

		if (!File.Exists(FilePath))
		{
			IsValid = true;
			return;
		}

		if (ParseFileToMods(FilePath, keepVortexKey))
			IsValid = true;
	}

	internal bool ParseFileToMods(string filePath, bool keepVortexKey)
	{
		List<ModLoadSetting> mods = [];
		ModLoadSetting currentMod = null;

		string[] lines = File.ReadAllLines(filePath);

		foreach (string line in lines)
		{
			string trimmedLine = line.Trim();

			if (string.IsNullOrEmpty(trimmedLine))
				continue; // Skip empty lines

			if (trimmedLine.StartsWith("[", StringComparison.Ordinal) && trimmedLine.EndsWith("]", StringComparison.Ordinal))
			{
				// New mod section. If we had a previous mod, add it to the list
				if (currentMod != null)
				{
					if (!currentMod.Priority.HasValue || !currentMod.IsEnabled.HasValue)
					{
						ShowWarningForMalformedFile($"{currentMod.ModName} settings are incomplete. 'Enabled' and 'Priority' are both required.");
						return false;
					}

					mods.Add(currentMod);
				}

				// Create new mod
				currentMod = new ModLoadSetting(trimmedLine.TrimStart('[').TrimEnd(']'));
			}
			else
			{
				// This is a key=value line
				string[] parts = trimmedLine.Split('=');
				if (parts.Length != 2)
				{
					ShowWarningForMalformedFile($"Unrecognized setting or value for mod {currentMod.ModName},on line:\n\n{line}");
					return false;
				}

				string key = parts[0];
				string value = parts[1];

				if (key.Equals("Priority", StringComparison.OrdinalIgnoreCase))
				{
					if (!int.TryParse(value, out int priority) || TopPriority > priority || priority > BottomPriority)
					{
						ShowWarningForMalformedFile($"The priority for mod {currentMod.ModName} isn't within the valid range of {TopPriority} to {BottomPriority}:\n\n{line}");
						return false;
					}

					currentMod.Priority = priority;
				}
				else if (key.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
				{
					if (!value.Equals("0", StringComparison.Ordinal) && !value.Equals("1", StringComparison.Ordinal))
					{
						ShowWarningForMalformedFile($"The 'Enabled' setting for mod {currentMod.ModName} isn't within the valid range of 0 or 1:\n\n{line}");
						return false;
					}

					currentMod.IsEnabled = value.Equals("1", StringComparison.Ordinal);
				}
				else if (key.Equals("VK", StringComparison.OrdinalIgnoreCase))
				{
					if (keepVortexKey)
						currentMod.VK = value;
				}
				else
				{
					ShowWarningForMalformedFile($"Unrecognized setting for mod {currentMod.ModName},on line:\n\n{line}");
					return false;
				}
			}
		}

		// Add last mod to list
		if (currentMod != null)
		{
			if (!currentMod.Priority.HasValue || !currentMod.IsEnabled.HasValue)
			{
				ShowWarningForMalformedFile($"{currentMod.ModName} settings are incomplete. 'Enabled' and 'Priority' are both required.");
				return false;
			}

			mods.Add(currentMod);
		}

		Mods = mods;
		return true;
	}

	private static void ShowWarningForMalformedFile(string reason)
	{
		_ = MainFrm.uiThreadManager.ShowMessage(
			"Your mods.settings file is invalid.\n\n" + reason,
			"Invalid Load Order File",
			System.Windows.Forms.MessageBoxButtons.OK,
			System.Windows.Forms.MessageBoxIcon.Warning);
	}

	internal void Save()
	{
		StringBuilder builder = new();

		foreach (ModLoadSetting modSetting in Mods)
		{
			_ = builder
				.Append('[').Append(modSetting.ModName).AppendLine("]")
				.Append("Enabled=").AppendLine(Convert.ToInt32(modSetting.IsEnabled, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture))
				.Append("Priority=").AppendLine(modSetting.Priority?.ToString(CultureInfo.InvariantCulture));

			if (!string.IsNullOrEmpty(modSetting.VK))
			{
				_ = builder.Append("VK=").AppendLine(modSetting.VK);
			}

			if (modSetting != Mods.Last())
				_ = builder.AppendLine();
		}

		LoadOrderWatcher.EnableRaisingEvents = false;
		File.WriteAllText(FilePath, builder.ToString());
		LoadOrderWatcher.EnableRaisingEvents = true;
	}


	#endregion

	internal void AddMergedModIfMissingAndSave()
	{
		string mergedModName = Paths.RetrieveMergedModName();
		if (Mods.All(setting => !setting.ModName.EqualsIgnoreCase(mergedModName)))
		{
			AddMod(new ModLoadSetting
			{
				ModName = mergedModName,
				IsEnabled = true,
				Priority = TopPriority
			});
		}

		Save();
	}

	/// <summary>
	/// Checks if a conflict between the given mods has been resolved.
	/// A conflict is considered resolved if any of the following conditions are met:
	/// 1. At least one of the mods is enabled.
	/// 2. The count of mod load settings is greater than or equal to the total number of mods minus one. This ensures that at least all but one of the mods are accounted for in the load settings.
	/// If none of the mods exist or no conditions are met, the method returns false indicating that the conflict is not resolved.
	/// </summary>
	/// <param name="modNames">An enumeration of mod names.</param>
	/// <returns>True if the conflict between the mods has been resolved, otherwise false.</returns>
	internal bool HasResolvedConflict(IEnumerable<string> modNames)
	{
		IEnumerable<ModLoadSetting> loadSettings = modNames.Select(GetModLoadSettingByName).Where(setting => setting != null);
		if (!loadSettings.Any())
			return false;
		if (loadSettings.Any(setting => setting.IsEnabled.Value))
			return true;
		int numSettings = loadSettings.Count();
		int numMods = modNames.Count();

		return numSettings >= numMods - 1;
	}

	internal bool ContainsMod(string modName) => Mods.Any(setting => setting.ModName.EqualsIgnoreCase(modName));

	internal ModLoadSetting GetTopPriorityEnabledMod() =>
		Mods.OrderBy(setting => setting, new LoadOrderComparer()).FirstOrDefault();

	internal string GetTopPriorityEnabledMod(IEnumerable<string> conflictMods)
	{
		IEnumerable<ModLoadSetting> conflictModSettings = Mods.Where(setting => conflictMods.Any(modName => modName.EqualsIgnoreCase(setting.ModName)));
		IEnumerable<ModLoadSetting> enabledModSettings = conflictModSettings.Where(setting => setting.IsEnabled.Value);

		return !conflictModSettings.Any()
			? conflictMods
				.OrderBy(name => name, new LoadOrderComparer())
				.FirstOrDefault()
			: !enabledModSettings.Any()
			? conflictMods
				.Except(conflictModSettings.Select(setting => setting.ModName))
				.OrderBy(name => name, new LoadOrderComparer())
				.FirstOrDefault()
			: (enabledModSettings
			.OrderBy(setting => setting, new LoadOrderComparer())
			.ThenBy(setting => setting.ModName, new LoadOrderComparer())
			.FirstOrDefault()
			?.ModName);
	}

	internal ModLoadSetting GetModLoadSettingByName(string modName) => Mods.FirstOrDefault(setting => setting.ModName.EqualsIgnoreCase(modName));

	internal bool IsModDisabledByName(string modName)
	{
		ModLoadSetting mod = GetModLoadSettingByName(modName);
		return mod != null && !mod.IsEnabled.Value;
	}

	internal void ToggleModByName(string modName)
	{
		Refresh();
		ModLoadSetting mod = GetModLoadSettingByName(modName);

		if (mod != null)
		{
			mod.IsEnabled = !mod.IsEnabled;
		}
		else
		{
			AddMod(new ModLoadSetting
			{
				ModName = modName,
				IsEnabled = false,
				Priority = BottomPriority
			});
		}

		AddMergedModIfMissingAndSave();
	}

	internal int GetPriorityByName(string modName)
	{
		ModLoadSetting mod = GetModLoadSettingByName(modName);
		return mod != null ? mod.Priority.Value : -1;
	}

	internal void SetPriorityByName(string modName, int priority)
	{
		Refresh();
		ModLoadSetting mod = GetModLoadSettingByName(modName);

		if (mod != null)
		{
			List<ModLoadSetting> updatedMods = Mods.Select(m => m.ModName.EqualsIgnoreCase(modName) ?
				new ModLoadSetting
				{
					ModName = m.ModName,
					IsEnabled = m.IsEnabled,
					VK = m.VK,
					Priority = priority
				} : m).ToList();
			Mods = updatedMods;
			ModLoadSetting newMod = Mods.First(m => m.ModName.EqualsIgnoreCase(modName));
			IncrementLeadingContiguousPriorities(newMod);
		}
		else
		{
			ModLoadSetting newMod = new()
			{
				ModName = modName,
				IsEnabled = true,
				Priority = priority
			};
			AddMod(newMod);
		}

		AddMergedModIfMissingAndSave();
	}

	internal void AddMod(ModLoadSetting modLoadSetting, bool keepVortexKey = false)
	{
		if (keepVortexKey)
			Refresh(true);
		IncrementLeadingContiguousPriorities(modLoadSetting);
		Mods = Mods.Concat(new[] { modLoadSetting }).ToList();
		AddMergedModIfMissingAndSave();
		if (keepVortexKey)
			Refresh(false);
	}

	internal void RemoveMod(string modName)
	{
		Refresh();

		ModLoadSetting modToRemove = Mods.FirstOrDefault(setting => setting.ModName.EqualsIgnoreCase(modName));
		if (modToRemove != null)
		{
			Mods = Mods.Where(mod => mod != modToRemove).ToList();
			Save();
		}
	}

	private void IncrementLeadingContiguousPriorities(ModLoadSetting incomingMod)
	{
		IncrementLeadingContiguousPrioritiesInternal(incomingMod);
		Mods = Mods; // Re-sort list
	}

	private void IncrementLeadingContiguousPrioritiesInternal(ModLoadSetting incomingMod)
	{
		int? startingPriority = incomingMod.Priority;
		if (startingPriority is null)
			return;
		int? nextPriority = startingPriority + 1;
		ModLoadSetting[] modsToIncrement = Mods.Where(mod => mod.Priority == startingPriority && mod.ModName != incomingMod.ModName).ToArray();
		ModLoadSetting[] displacedMods = Mods.Where(mod => mod.Priority == nextPriority && mod.ModName != incomingMod.ModName).ToArray();

		if (!modsToIncrement.Any())
			return;

		if (displacedMods.Any() &&
			nextPriority < BottomPriority)
		{
			IncrementLeadingContiguousPrioritiesInternal(new ModLoadSetting { ModName = incomingMod.ModName, Priority = nextPriority });
		}

		foreach (ModLoadSetting mod in modsToIncrement)
			++mod.Priority;
	}
}

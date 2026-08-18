using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Xml.Linq;

using WitcherScriptMerger.Utilities;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger;

/// <summary>
/// Manages application settings using the App.config file.
/// </summary>
internal class AppSettings
{
	private readonly string _assemblyPath;
	private Configuration _cachedConfig;

	/// <summary>
	/// Gets a value indicating whether a configuration file exists.
	/// </summary>
	internal bool HasConfigFile => CachedConfig.HasFile;

	private Configuration CachedConfig
	{
		get
		{
			_cachedConfig ??= ConfigurationManager.OpenExeConfiguration(_assemblyPath);
			return _cachedConfig;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AppSettings"/> class.
	/// </summary>
	internal AppSettings()
	{
		_assemblyPath = Assembly.GetEntryAssembly().Location;
		_cachedConfig = ConfigurationManager.OpenExeConfiguration(_assemblyPath);

		if (!HasConfigFile)
			CreateDefaultAppConfig();
	}

	/// <summary>
	/// Creates a default App.config file with default settings.
	/// </summary>
	private void CreateDefaultAppConfig()
	{
		// Create the XML document using LINQ to XML
		XDocument doc = new(
			new XDeclaration("1.0", "utf-8", "yes"),
			new XElement("configuration",
				new XElement("appSettings",
					new XElement("add", new XAttribute("key", "GameDirectory"), new XAttribute("value", "")),
					new XElement("add", new XAttribute("key", "VanillaScriptsDirectory"), new XAttribute("value", "")),
					new XElement("add", new XAttribute("key", "ModsDirectory"), new XAttribute("value", "")),
					new XElement("add", new XAttribute("key", "CheckScripts"), new XAttribute("value", DefaultSettings.CheckScripts.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "CheckXmlFiles"), new XAttribute("value", DefaultSettings.CheckXmlFiles.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "CheckBundles"), new XAttribute("value", DefaultSettings.CheckBundles)),
					new XElement("add", new XAttribute("key", "IgnoreModNames"), new XAttribute("value", DefaultSettings.IgnoreModNames)),
					new XElement("add", new XAttribute("key", "CollapseIdenticalConflicts"), new XAttribute("value", DefaultSettings.CollapseIdenticalConflicts.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "CollapseCustomLoadOrder"), new XAttribute("value", DefaultSettings.CollapseCustomLoadOrder.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "CollapseNotMergeable"), new XAttribute("value", DefaultSettings.CollapseNotMergeable.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "ValidateMergeSources"), new XAttribute("value", DefaultSettings.ValidateMergeSources.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "ValidateCustomLoadOrder"), new XAttribute("value", DefaultSettings.ValidateCustomLoadOrder.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "ReviewEachMerge"), new XAttribute("value", DefaultSettings.ReviewEachMerge.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "CheckDuplicatePrios"), new XAttribute("value", DefaultSettings.CheckDuplicatePrios.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "PlayCompletionSounds"), new XAttribute("value", DefaultSettings.PlayCompletionSounds.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "ReportAfterMerge"), new XAttribute("value", DefaultSettings.ReportAfterMerge.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "ReportAfterPack"), new XAttribute("value", DefaultSettings.ReportAfterPack.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "AutoCreateScriptMerges"), new XAttribute("value", DefaultSettings.AutoCreateScriptMerges.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "AutoDeleteOldMerges"), new XAttribute("value", DefaultSettings.AutoDeleteOldMerges.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "AutoOverwriteOldMerges"), new XAttribute("value", DefaultSettings.AutoOverwriteOldMerges.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "AutoSkipKDiff3InfoDialogs"), new XAttribute("value", DefaultSettings.AutoSkipKDiff3InfoDialogs.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "AutoExit"), new XAttribute("value", DefaultSettings.AutoExit.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "AutoBackupLoadOrder"), new XAttribute("value", DefaultSettings.AutoBackupLoadOrder.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "ColorTheme"), new XAttribute("value", DefaultSettings.ColorTheme)),
					new XElement("add", new XAttribute("key", "AccentColor"), new XAttribute("value", DefaultSettings.AccentColor.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "PrioToolTips"), new XAttribute("value", DefaultSettings.PrioTooltips)),
					new XElement("add", new XAttribute("key", "NoConflictsWitcher"), new XAttribute("value", DefaultSettings.NoConflictsWitcher.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "RanThroughVortex"), new XAttribute("value", DefaultSettings.RanThroughVortex.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "MergedModName"), new XAttribute("value", DefaultSettings.MergedModName)),
					new XElement("add", new XAttribute("key", "KDiff3Path"), new XAttribute("value", DefaultSettings.KDiff3Path)),
					new XElement("add", new XAttribute("key", "QuickBmsPath"), new XAttribute("value", DefaultSettings.QuickBmsPath)),
					new XElement("add", new XAttribute("key", "QuickBmsPluginPath"), new XAttribute("value", DefaultSettings.QuickBmsPluginPath)),
					new XElement("add", new XAttribute("key", "WccLitePath"), new XAttribute("value", DefaultSettings.WccLitePath)),
					new XElement("add", new XAttribute("key", "StartMaximized"), new XAttribute("value", DefaultSettings.StartMaximized.ToString().ToUpperInvariant())),
					new XElement("add", new XAttribute("key", "StartWidth"), new XAttribute("value", DefaultSettings.StartWidth)),
					new XElement("add", new XAttribute("key", "StartHeight"), new XAttribute("value", DefaultSettings.StartHeight)),
					new XElement("add", new XAttribute("key", "StartPosTop"), new XAttribute("value", DefaultSettings.StartPosTop)),
					new XElement("add", new XAttribute("key", "StartPosLeft"), new XAttribute("value", DefaultSettings.StartPosLeft))
				)
			)
		);

		// Save the XML document to the file
		doc.Save(CachedConfig.FilePath, SaveOptions.None);

		// Reload the configuration after creating the file
		_cachedConfig = ConfigurationManager.OpenExeConfiguration(_assemblyPath);
	}

	/// <summary>
	/// Sets the value of an application setting.
	/// </summary>
	internal void Set(string key, object value)
	{
		try
		{
			CachedConfig.AppSettings.Settings[key].Value = value.ToString();
		}
		catch (Exception ex) when (ex is KeyNotFoundException or FormatException or NullReferenceException ||
								   ex.InnerException is KeyNotFoundException or FormatException or NullReferenceException)
		{
			CachedConfig.AppSettings.Settings.Add(key, value.ToString());
		}
	}

	/// <summary>
	/// Gets the value of an application setting with the specified type.
	/// </summary>
	internal T Get<T>(string key, T defaultValue = default)
	{
		try
		{
			if (!HasConfigFile)
				CreateDefaultAppConfig();

			string valueString = CachedConfig.AppSettings.Settings[key].Value;
			MethodInfo parseMethod = typeof(T).GetMethod("Parse", [typeof(string)]);
			object valueObject = parseMethod.Invoke(null, [valueString]);
			return (T)valueObject;
		}
		catch (Exception ex) when (ex is KeyNotFoundException or FormatException or NullReferenceException ||
								   ex.InnerException is KeyNotFoundException or FormatException or NullReferenceException)
		{
			return defaultValue;
		}
	}

	/// <summary>
	/// Gets the value of an application setting as a string.
	/// </summary>
	internal string Get(string key)
	{
		try
		{
			if (!HasConfigFile)
				CreateDefaultAppConfig();

			return CachedConfig.AppSettings.Settings[key].Value;
		}
		catch (Exception ex) when (ex is KeyNotFoundException or FormatException or NullReferenceException ||
								   ex.InnerException is KeyNotFoundException or FormatException or NullReferenceException)
		{
			return string.Empty;
		}
	}

	/// <summary>
	/// Saves the application settings to the configuration file.
	/// </summary>
	internal void Save()
	{
		try
		{
			CachedConfig.Save(ConfigurationSaveMode.Minimal);
		}
		catch (ConfigurationErrorsException ex)
		{
			_ = MainFrm.uiThreadManager.ShowError($"Failed to save config due to error:\n\n{ex.Message}");
		}
	}
}
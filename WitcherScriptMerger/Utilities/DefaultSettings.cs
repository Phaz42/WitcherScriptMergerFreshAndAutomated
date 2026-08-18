namespace WitcherScriptMerger.Utilities;

internal class DefaultSettings
{
	internal const bool CheckScripts = true;
	internal const bool CheckXmlFiles = true;
	internal const int CheckBundles = 1;
	internal const string IgnoreModNames = "";
	internal const bool CollapseIdenticalConflicts = true;
	internal const bool CollapseNotMergeable = false;
	internal const bool CollapseCustomLoadOrder = false;
	internal const bool ValidateMergeSources = true;
	internal const bool ValidateCustomLoadOrder = true;
	internal const bool ReviewEachMerge = false;
	internal const bool CheckDuplicatePrios = true;
	internal const bool PlayCompletionSounds = false;
	internal const bool ReportAfterMerge = false;
	internal const bool ReportAfterPack = false;
	internal const bool AutoCreateScriptMerges = false;
	internal const bool AutoDeleteOldMerges = true;
	internal const bool AutoOverwriteOldMerges = true;
	internal const bool AutoSkipKDiff3InfoDialogs = true;
	internal const bool AutoExit = false;
	internal const bool AutoBackupLoadOrder = true;
	internal const int ColorTheme = 0;
	internal const bool AccentColor = true;
	internal const int PrioTooltips = 0;
	internal const bool NoConflictsWitcher = true;
	internal const bool RanThroughVortex = false;
	internal const string MergedModName = "mod0000_MergedFiles";
	internal const string KDiff3Path = @"Tools\KDiff3\KDiff3.exe";
	internal const string QuickBmsPath = @"Tools\QuickBMS\quickbms.exe";
	internal const string QuickBmsPluginPath = @"Tools\QuickBMS\witcher3.bms";
	internal const string WccLitePath = @"Tools\wcc_lite\bin\x64\wcc_lite.exe";
	internal const bool StartMaximized = false;
	internal const string StartWidth = "";
	internal const string StartHeight = "";
	internal const string StartPosTop = "";
	internal const string StartPosLeft = "";
}

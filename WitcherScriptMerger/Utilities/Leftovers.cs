using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace WitcherScriptMerger.Utilities;

/// <summary>
/// Removes, at start, the files earlier releases shipped that this one no longer needs, so that a new version
/// extracted over an old one leaves nothing behind. The list, Leftovers.tsv, is generated from the released packages
/// (SM-FAE's build tooling): each path with every SHA-256 it had. A listed file is deleted only when its content matches
/// one of them, so a file the user changed or put there stays; then the folders that leaves empty go. Never anything
/// under Tools\ (a StandAlone extracted over a Full install keeps the Full's tools), a .config file, MergeInventory.xml
/// or the user's own images. A file that can't be deleted is left as it is.
/// </summary>
internal static class Leftovers
{
	private const string ListResource = "WitcherScriptMerger.Leftovers.tsv";

	/// <summary>
	/// Removes the leftovers from SM-FAE's own folder. Returns how many files were deleted.
	/// </summary>
	internal static int Remove()
	{
		string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(AppContext.BaseDirectory));
		int deleted = 0;
		HashSet<string> folders = new(StringComparer.OrdinalIgnoreCase);
		foreach ((string relativePath, HashSet<string> hashes) in ReadList())
		{
			string path = InFolder(root, relativePath);
			if (path == null || IsGuarded(relativePath))
				continue;
			string folder = Path.GetDirectoryName(path);
			if (!string.Equals(folder, root, StringComparison.OrdinalIgnoreCase))
				_ = folders.Add(folder);
			try
			{
				FileInfo file = new(path);
				if (!file.Exists || file.Attributes.HasFlag(FileAttributes.ReparsePoint))
					continue;
				if (!hashes.Contains(Sha256(path)))
					continue;
				file.Delete();
				deleted++;
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// In use, read-only or not ours to delete: left as it is.
			}
		}

		// The deepest folders first, so that a folder emptied by its subfolders' removal goes too.
		foreach (string folder in folders.SelectMany(Ancestors(root)).Distinct(StringComparer.OrdinalIgnoreCase)
			.OrderByDescending(f => f.Length))
		{
			try
			{
				DirectoryInfo dir = new(folder);
				if (dir.Exists && !dir.Attributes.HasFlag(FileAttributes.ReparsePoint) && !dir.EnumerateFileSystemInfos().Any())
					dir.Delete();
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// Left as it is.
			}
		}
		return deleted;
	}

	/// <summary>The path under SM-FAE's folder, or null for anything that would lead outside it.</summary>
	private static string InFolder(string root, string relativePath)
	{
		if (Path.IsPathRooted(relativePath))
			return null;
		string path = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
		return path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ? path : null;
	}

	/// <summary>The paths never deleted, whatever the list says.</summary>
	internal static bool IsGuarded(string relativePath)
	{
		string path = relativePath.Replace('\\', '/');
		return path.StartsWith("Tools/", StringComparison.OrdinalIgnoreCase)
			|| path.StartsWith("Images/Custom/", StringComparison.OrdinalIgnoreCase)
			|| path.EndsWith(".config", StringComparison.OrdinalIgnoreCase)
			|| path.Equals("MergeInventory.xml", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>A folder and its parents, up to but not including SM-FAE's folder.</summary>
	private static Func<string, IEnumerable<string>> Ancestors(string root) => folder =>
	{
		List<string> chain = [];
		for (string f = folder; f != null && f.Length > root.Length; f = Path.GetDirectoryName(f))
			chain.Add(f);
		return chain;
	};

	private static IEnumerable<(string, HashSet<string>)> ReadList()
	{
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ListResource);
		if (stream == null)
			yield break;
		using StreamReader reader = new(stream);
		while (reader.ReadLine() is string line)
		{
			if (line.Length == 0 || line[0] == '#')
				continue;
			string[] parts = line.Split('\t');
			if (parts.Length == 2)
				yield return (parts[0], new HashSet<string>(parts[1].Split(','), StringComparer.OrdinalIgnoreCase));
		}
	}

	private static string Sha256(string path)
	{
		using FileStream stream = File.OpenRead(path);
		return Convert.ToHexStringLower(SHA256.HashData(stream));
	}
}

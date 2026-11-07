using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace WitcherScriptMerger.Utilities;

/// <summary>
/// Removes the files earlier releases shipped that this one no longer needs, so that a new version extracted over an
/// old one leaves nothing behind; run once per version (Program). The list, Leftovers.tsv, is generated from the
/// released packages (SM-FAE's build tooling): each path with every SHA-256 it had. A listed file is deleted only when
/// its content matches one of them, so a file the user changed or put there stays; then the folders its deletion left
/// empty. Never anything under Tools\ (a StandAlone extracted over a Full install keeps the Full's tools), a .config
/// file, MergeInventory.xml or the user's own images, however the list spells the path, and never through a link. A file
/// that can't be deleted is left as it is.
/// </summary>
internal static class Leftovers
{
	private const string ListResource = "WitcherScriptMerger.Leftovers.tsv";

	/// <summary>
	/// Removes the leftovers from SM-FAE's own folder. Returns how many files were deleted.
	/// </summary>
	internal static int Remove()
	{
		// A drive root keeps its separator ("D:\"); any other folder has none.
		string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(AppContext.BaseDirectory));
		string prefix = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;
		int deleted = 0;
		HashSet<string> emptied = new(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, bool> linked = new(StringComparer.OrdinalIgnoreCase);
		foreach ((string relativePath, HashSet<string> hashes) in ReadList())
		{
			string path = InFolder(prefix, relativePath);
			// The guards are applied to the path as it is used, not as the list spells it ("./Tools/x", "a/../x.config.").
			if (path == null || IsGuarded(Path.GetRelativePath(root, path)) || ThroughLink(prefix, path, linked))
				continue;
			try
			{
				FileInfo file = new(path);
				if (!file.Exists || file.Attributes.HasFlag(FileAttributes.ReparsePoint))
					continue;
				// Hashed, then deleted by path: a file swapped in between the two, by a program writing into this folder
				// at that instant, would go unhashed. Accepted: only another program writing here could do it, and
				// anything that can write here can delete as well.
				if (!hashes.Contains(Sha256(path)))
					continue;
				file.Delete();
				deleted++;
				_ = emptied.Add(Path.GetDirectoryName(path));
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// In use, read-only or not ours to delete: left as it is.
			}
		}

		// Only the folders a deletion above left empty, and their parents, the deepest first.
		foreach (string folder in emptied.SelectMany(f => Ancestors(prefix, f)).Distinct(StringComparer.OrdinalIgnoreCase)
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
	private static string InFolder(string prefix, string relativePath)
	{
		if (Path.IsPathRooted(relativePath))
			return null;
		string path = Path.GetFullPath(Path.Combine(prefix, relativePath.Replace('/', Path.DirectorySeparatorChar)));
		return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && path.Length > prefix.Length ? path : null;
	}

	/// <summary>The paths never deleted, whatever the list says, given relative to SM-FAE's folder.</summary>
	internal static bool IsGuarded(string relativePath)
	{
		string path = relativePath.Replace('\\', '/');
		return path.StartsWith("Tools/", StringComparison.OrdinalIgnoreCase)
			|| path.StartsWith("Images/Custom/", StringComparison.OrdinalIgnoreCase)
			|| path.EndsWith(".config", StringComparison.OrdinalIgnoreCase)
			|| path.Equals("MergeInventory.xml", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Whether any folder between SM-FAE's folder and the file is a link (a junction or a symbolic link), which would
	/// lead the deletion somewhere else. Each folder is looked at once.
	/// </summary>
	private static bool ThroughLink(string prefix, string path, Dictionary<string, bool> linked)
	{
		foreach (string folder in Ancestors(prefix, Path.GetDirectoryName(path)))
		{
			if (!linked.TryGetValue(folder, out bool isLink))
			{
				try
				{
					DirectoryInfo dir = new(folder);
					isLink = dir.Exists && dir.Attributes.HasFlag(FileAttributes.ReparsePoint);
				}
				catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
				{
					isLink = true;   // Unreadable: treated as a link, so nothing below it is touched.
				}
				linked[folder] = isLink;
			}
			if (isLink)
				return true;
		}
		return false;
	}

	/// <summary>A folder and its parents, up to but not including SM-FAE's folder.</summary>
	private static List<string> Ancestors(string prefix, string folder)
	{
		List<string> chain = [];
		for (string f = folder; f != null && f.Length > prefix.Length - 1 && !Path.EndsInDirectorySeparator(f); f = Path.GetDirectoryName(f))
			if (f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				chain.Add(f);
		return chain;
	}

	private static IEnumerable<(string, HashSet<string>)> ReadList()
	{
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ListResource);
		if (stream == null)
			yield break;
		using StreamReader reader = new(stream);
		int number = 0;
		while (reader.ReadLine() is string line)
		{
			number++;
			if (line.Trim().Length == 0 || line[0] == '#')
				continue;
			string[] parts = line.Split('\t', StringSplitOptions.TrimEntries);
			string[] hashes = parts.Length == 2 ? parts[1].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) : [];
			if (parts.Length != 2 || parts[0].Length == 0 || hashes.Length == 0)
			{
				Program.db($"Leftovers.tsv, line {number}: not a path and its hashes; skipped");
				continue;
			}
			yield return (parts[0], new HashSet<string>(hashes, StringComparer.OrdinalIgnoreCase));
		}
	}

	private static string Sha256(string path)
	{
		using FileStream stream = File.OpenRead(path);
		return Convert.ToHexStringLower(SHA256.HashData(stream));
	}
}

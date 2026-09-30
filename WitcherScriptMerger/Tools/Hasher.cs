using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.IO.Hashing;

namespace WitcherScriptMerger.Tools;

/// <summary>
/// The xxHash32 (seed 0) of a file's content, as the hexadecimal text the merge inventory stores.
/// </summary>
internal static class Hasher
{
	// A bundle merge hashes the mod's whole bundle: once per merged file, and again at each check on every refresh.
	// While a file's length and last write time are unchanged, its content is taken as unchanged too, so it is hashed
	// once per run of SM-FAE. A mod file that is updated or replaced gets a new write time and is hashed again.
	private static readonly ConcurrentDictionary<(string Path, long Length, DateTime LastWriteUtc), string> Hashes = new();

	internal static string ComputeHash(string filePath)
	{
		FileInfo file = new(filePath);
		if (!file.Exists)
			throw new FileNotFoundException("Can't find file to hash:  " + filePath);

		return Hashes.GetOrAdd((file.FullName.ToUpperInvariant(), file.Length, file.LastWriteTimeUtc), key => HashFile(file.FullName));
	}

	// System.IO.Hashing gives exactly the values of the managed xxHash32 SM-FAE used before - compared on 308 inputs, every
	// length from 0 to 300 bytes among them - so existing inventories stay valid. It reads the file in large blocks,
	// where the old one read four bytes per call: about seven times faster on a large bundle.
	private static string HashFile(string path)
	{
		XxHash32 hash = new();
		using (FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20))
			hash.Append(stream);

		return string.Format(CultureInfo.InvariantCulture, "{0:X}", hash.GetCurrentHashAsUInt32());
	}
}

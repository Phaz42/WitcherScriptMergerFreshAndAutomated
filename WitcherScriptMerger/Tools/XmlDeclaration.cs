using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace WitcherScriptMerger.Tools;

/// <summary>
/// The encoding named in an XML declaration, which SM-FAE owns for every XML file it merges: the declaration must name
/// the encoding the file is actually in. KDiff3 merges line 1 like any other line, so left to it, a mod saved the 4.x
/// way (UTF-16, declaring UTF-16) merged beside a converted one gives a UTF-8 file that declares UTF-16, or the reverse.
/// </summary>
internal static class XmlDeclaration
{
	internal enum TextForm { Utf8, Utf8Bom, Utf16LE, Utf16BE, Other }

	// The encoding attribute of an XML declaration at the very start of the text; group 2 is its value.
	private static readonly Regex EncodingAttribute =
		new(@"\A<\?xml[^>]*?\bencoding\s*=\s*([""'])([^""']*)\1", RegexOptions.CultureInvariant);

	private static readonly UTF8Encoding StrictUtf8 = new(false, true);

	internal static TextForm DetectForm(byte[] bytes)
	{
		if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
			return TextForm.Utf8Bom;
		if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
			return TextForm.Utf16LE;
		if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
			return TextForm.Utf16BE;

		try
		{
			_ = StrictUtf8.GetString(bytes);
			return TextForm.Utf8;
		}
		catch (DecoderFallbackException)
		{
			return TextForm.Other;
		}
	}

	/// <summary>KDiff3's name for writing a file in this form (measured on KDiff3 0.9.98: "UTF-16LE" writes the mark).</summary>
	internal static string KDiff3Codec(TextForm form) => form switch
	{
		TextForm.Utf8Bom => "UTF-8-BOM",
		TextForm.Utf16LE => "UTF-16LE",
		TextForm.Utf16BE => "UTF-16BE",
		_ => "UTF-8",
	};

	/// <summary>
	/// The encoding name a declaration should carry in a file of this form: the vanilla file's own spelling when it has
	/// one that fits, otherwise UTF-8 or UTF-16.
	/// </summary>
	internal static string DeclaredName(TextForm form, string vanillaPath)
	{
		string fallback = IsUtf16(form) ? "UTF-16" : "UTF-8";
		if (vanillaPath == null)
			return fallback;

		byte[] vanillaBytes = File.ReadAllBytes(vanillaPath);
		Match match = EncodingAttribute.Match(Decode(vanillaBytes, DetectForm(vanillaBytes)));
		return match.Success && Fits(match.Groups[2].Value, form) ? match.Groups[2].Value : fallback;
	}

	/// <summary>
	/// Copies a file with the encoding its declaration names set to <paramref name="encodingName"/>, keeping the file's
	/// own encoding and everything else byte for byte. A file with no declaration, or none naming an encoding, is copied as it is.
	/// </summary>
	internal static void CopyWithEncoding(string sourcePath, string destPath, string encodingName)
	{
		_ = Directory.CreateDirectory(Path.GetDirectoryName(destPath));

		byte[] bytes = File.ReadAllBytes(sourcePath);
		TextForm form = DetectForm(bytes);
		string text = Decode(bytes, form);
		Match match = EncodingAttribute.Match(text);
		if (!match.Success)
		{
			File.Copy(sourcePath, destPath, true);
			return;
		}

		Group value = match.Groups[2];
		string newText = string.Concat(text.AsSpan(0, value.Index), encodingName, text.AsSpan(value.Index + value.Length));
		File.WriteAllBytes(destPath, Encode(newText, form));
	}

	/// <summary>Null when the file's declaration fits its encoding, or it has none; otherwise what does not fit.</summary>
	internal static string FindMismatch(string path)
	{
		byte[] bytes = File.ReadAllBytes(path);
		TextForm form = DetectForm(bytes);
		Match match = EncodingAttribute.Match(Decode(bytes, form));
		if (!match.Success || Fits(match.Groups[2].Value, form))
			return null;

		return $"declares {match.Groups[2].Value} but is {Describe(form)}";
	}

	// Only UTF-8 and UTF-16 are judged: those are what KDiff3 writes and what the game's own XML uses. A declaration
	// naming anything else is left as the mod author wrote it.
	private static bool Fits(string declared, TextForm form)
	{
		string name = declared.Trim();
		if (name.StartsWith("UTF-16", StringComparison.OrdinalIgnoreCase) || name.EqualsIgnoreCase("UNICODE"))
			return IsUtf16(form);
		if (name.EqualsIgnoreCase("UTF-8") || name.EqualsIgnoreCase("UTF8"))
			return form is TextForm.Utf8 or TextForm.Utf8Bom;
		return true;
	}

	private static bool IsUtf16(TextForm form) => form is TextForm.Utf16LE or TextForm.Utf16BE;

	private static string Describe(TextForm form) => form switch
	{
		TextForm.Utf8 => "UTF-8",
		TextForm.Utf8Bom => "UTF-8 with a byte order mark",
		TextForm.Utf16LE => "UTF-16 (little endian)",
		TextForm.Utf16BE => "UTF-16 (big endian)",
		_ => "not UTF-8 or UTF-16",
	};

	// An unknown encoding is read as Latin-1, which maps every byte to one character and back, so the file round-trips
	// unchanged apart from the declaration, which is ASCII in every encoding the game uses.
	private static string Decode(byte[] bytes, TextForm form) => form switch
	{
		TextForm.Utf8Bom => Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3),
		TextForm.Utf16LE => Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2),
		TextForm.Utf16BE => Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2),
		TextForm.Utf8 => Encoding.UTF8.GetString(bytes),
		_ => Encoding.Latin1.GetString(bytes),
	};

	private static byte[] Encode(string text, TextForm form) => form switch
	{
		TextForm.Utf8Bom => [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(text)],
		TextForm.Utf16LE => [.. Encoding.Unicode.GetPreamble(), .. Encoding.Unicode.GetBytes(text)],
		TextForm.Utf16BE => [.. Encoding.BigEndianUnicode.GetPreamble(), .. Encoding.BigEndianUnicode.GetBytes(text)],
		TextForm.Utf8 => Encoding.UTF8.GetBytes(text),
		_ => Encoding.Latin1.GetBytes(text),
	};
}

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WitcherScriptMerger.UI;

/// <summary>
/// A search typed above the conflicts or merges list. It matches a file node - one conflict, or one merge - when its file
/// path matches, or when one of its mods does; the whole node is then shown, with all its mods.
/// <para>
/// One term, upper/lower case ignored, optionally limited with a prefix: <c>mod:</c> searches mod names only, <c>file:</c>
/// file paths only. A term without <c>*</c> or <c>?</c> matches anywhere in the name. A term with them is a pattern
/// (<c>*</c> any text, <c>?</c> one character) that must match the whole name - for a file, the whole path or the file
/// name alone, so <c>file:*Input.ws</c> finds <c>game\player\playerInput.ws</c>. A term in double quotes must equal the
/// whole name; "Show only this mod" uses it, so that modX does not also find modXAddon.
/// </para>
/// </summary>
internal sealed class TreeSearch
{
	private enum Field { Any, Mod, File }

	private readonly Field _field;
	private readonly Func<string, bool> _matchesText;

	private TreeSearch(Field field, Func<string, bool> matchesText)
	{
		_field = field;
		_matchesText = matchesText;
	}

	/// <summary>The search typed, or null when it searches for nothing - then everything is shown.</summary>
	internal static TreeSearch Parse(string text)
	{
		string term = (text ?? string.Empty).Trim();
		Field field = Field.Any;
		if (term.StartsWith("mod:", StringComparison.OrdinalIgnoreCase))
		{
			field = Field.Mod;
			term = term[4..].Trim();
		}
		else if (term.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
		{
			field = Field.File;
			term = term[5..].Trim();
		}

		// Paths in the lists use backslashes; a forward slash typed means the same.
		term = term.Replace('/', '\\');

		if (term.Length > 1 && term.StartsWith('"') && term.EndsWith('"'))
		{
			string exact = term[1..^1];
			return exact.Length == 0 ? null : new TreeSearch(field, name => name.Equals(exact, StringComparison.OrdinalIgnoreCase));
		}

		term = term.Trim('"');
		if (term.Length == 0)
			return null;

		if (term.IndexOfAny(['*', '?']) == -1)
			return new TreeSearch(field, name => name.Contains(term, StringComparison.OrdinalIgnoreCase));

		Regex pattern = new(
			"^" + Regex.Escape(term).Replace(@"\*", ".*", StringComparison.Ordinal).Replace(@"\?", ".", StringComparison.Ordinal) + "$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
		return new TreeSearch(field, pattern.IsMatch);
	}

	/// <summary>Whether this file node - a conflict or a merge, with its mods below it - is shown.</summary>
	internal bool MatchesFileNode(TreeNode fileNode)
	{
		if (_field != Field.Mod && MatchesFilePath(fileNode.Text))
			return true;

		return _field != Field.File && fileNode.Nodes.Cast<TreeNode>().Any(modNode => _matchesText(modNode.Text));
	}

	/// <summary>Whether a mod node is one the search found, so it can be marked among the other mods of its file.</summary>
	internal bool MatchesModName(string modName) => _field != Field.File && _matchesText(modName);

	// A pattern must match the whole path or the file name alone; a plain term found in the path is found in either.
	private bool MatchesFilePath(string relativePath) =>
		_matchesText(relativePath) || _matchesText(Path.GetFileName(relativePath));
}

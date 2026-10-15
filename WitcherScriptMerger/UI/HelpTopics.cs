using System.Collections.Generic;

namespace WitcherScriptMerger.UI;

/// <summary>How a line of a help topic is shown (HelpPanel).</summary>
internal enum HelpStyle
{
	/// <summary>A paragraph.</summary>
	Text,

	/// <summary>A section's heading.</summary>
	Heading,

	/// <summary>A point of a list.</summary>
	Bullet,

	/// <summary>Something to type or a name, in a fixed-width font, then what it does.</summary>
	Example
}

/// <summary>A line of a help topic; an Example line's Example is shown first, its Text after it.</summary>
internal sealed record HelpLine(HelpStyle Style, string Text, string Example = null);

/// <summary>A help topic: its title, and its lines.</summary>
internal sealed record HelpTopic(string Title, IReadOnlyList<HelpLine> Lines);

/// <summary>The help the "?" buttons open: how the search works, and how to use your own "no conflicts" image.</summary>
internal static class HelpTopics
{
	private static HelpLine Text(string text) => new(HelpStyle.Text, text);
	private static HelpLine Heading(string text) => new(HelpStyle.Heading, text);
	private static HelpLine Bullet(string text) => new(HelpStyle.Bullet, text);
	private static HelpLine Example(string example, string text) => new(HelpStyle.Example, text, example);

	// TreeSearch, TreeManager.SetSearchText and MainForm's keys are what this describes.
	internal static HelpTopic Search { get; } = new("Searching the lists",
	[
		Text("Each list has its own search. It shows only the conflicts or merges that match, each with all its mods, "
			+ "and marks what was found in green: a mod's name, or the part of a file's path that matched."),
		Heading("What to type"),
		Example("lights", "mods and files with \"lights\" anywhere in the name"),
		Example("mod:lights", "mod names only"),
		Example("file:input", "file paths only"),
		Example("file:*Input.ws", "a pattern: * is any text, ? is one character. A pattern must match a whole path or "
			+ "a whole file name, so this finds game\\player\\playerInput.ws."),
		Example("mod:\"modLights\"", "exactly this name, so modLightsAddon is not found"),
		Text("Upper and lower case don't matter, and / in a path means the same as \\."),
		Heading("While a search is shown"),
		Bullet("The count beside the box says how many conflicts or merges are shown, of how many, and how many selected "
			+ "ones the search hides."),
		Bullet("Merging and deleting always include selected items the search hides."),
		Bullet("Right-click a mod and choose Show Only This Mod's Conflicts, or Merges, to search for exactly that mod."),
		Heading("Keys"),
		Example("Ctrl+F", "search the list you're in, or the conflicts"),
		Example("Enter", "search at once, without the short pause after typing"),
		Example("Esc", "clear the search; press it again to go back to the list"),
		Text("The × in the box clears the search too."),
	]);

	// NoConflictsImage is what this describes; the folder is named as it is on this computer.
	internal static HelpTopic NoConflictsImage => new("Your own \"no conflicts\" image",
	[
		Text("When there are no conflicts, SM-FAE shows an image in place of the conflict list, while this option is on. "
			+ "You can show your own instead."),
		Heading("How"),
		Text("Put your image in this folder:"),
		Example(Theming.NoConflictsImage.CustomDirectory, string.Empty),
		Text("with one of these names:"),
		Example("No-Conflicts.png", "shown with both themes"),
		Example("No-Conflicts-Dark.png", "shown with the dark theme"),
		Example("No-Conflicts-Light.png", "shown with the light theme"),
		Text(".jpg, .jpeg and .gif work too. A theme's own image comes first: with No-Conflicts.png and "
			+ "No-Conflicts-Dark.png both there, the dark theme shows No-Conflicts-Dark.png and the light theme No-Conflicts.png."),
		Heading("Good to know"),
		Bullet("Animated GIFs play."),
		Bullet("A file larger than 20 MB, or one that isn't a readable image, is ignored."),
		Bullet("Without an image of your own for a theme, SM-FAE shows its own."),
		Bullet("A changed image shows at the next refresh, or when SM-FAE is restarted."),
		Bullet("Upgrading SM-FAE never puts anything in that folder but its README, so your images stay."),
	]);
}

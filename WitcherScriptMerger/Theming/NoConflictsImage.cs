using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;

namespace WitcherScriptMerger.Theming;

/// <summary>
/// The image shown in place of the conflict list when there are no conflicts. It ships as a file beside SM-FAE, never
/// built in, so that a user can replace it: Images\Custom is the user's own, and a release puts nothing there but its
/// README, so extracting an update over an install never overwrites a user's image. Images\Default ships with SM-FAE.
/// </summary>
internal static class NoConflictsImage
{
	// One name per theme and one for both, and four extensions. Deliberately no more: every extra naming option is one
	// more way for a user to name a file that is then silently ignored.
	private const string BothThemesName = "No-Conflicts";
	private static readonly string[] Extensions = [".png", ".jpg", ".jpeg", ".gif"];

	// A larger file is skipped rather than loaded: a huge image or animation would stall the main window.
	private const long MaxFileSize = 20 * 1024 * 1024;

	private static (string Path, long Length, DateTime LastWriteUtc) _loadedFrom;
	private static Image _loaded;

	internal static string CustomDirectory => Path.Combine(AppContext.BaseDirectory, "Images", "Custom");

	private static string DefaultDirectory => Path.Combine(AppContext.BaseDirectory, "Images", "Default");

	/// <summary>
	/// The image for the theme: the user's own in Images\Custom, else SM-FAE's in Images\Default, else null - then
	/// there is no image to show. In each folder the theme's own image (No-Conflicts-Dark or -Light) comes first, and
	/// the image for both themes (No-Conflicts) is used where the theme has none. So a user's image for both themes
	/// still wins over SM-FAE's own. A file that is missing, empty, too large or not a readable image is passed over.
	/// The same file, unchanged, is loaded once; a file replaced while SM-FAE runs is picked up at the next refresh.
	/// This class owns the image and disposes it when it loads another or finds none, so a caller shows only the image
	/// the latest call returned, and replaces it at once, on the UI thread, before anything repaints.
	/// </summary>
	internal static Image Get(bool darkTheme)
	{
		string themeName = darkTheme ? "No-Conflicts-Dark" : "No-Conflicts-Light";

		foreach (string directory in new[] { CustomDirectory, DefaultDirectory })
		{
			foreach (string name in new[] { themeName, BothThemesName })
			{
				foreach (string extension in Extensions)
				{
					FileInfo file = new(Path.Combine(directory, name + extension));
					if (!file.Exists || file.Length == 0 || file.Length > MaxFileSize)
						continue;

					(string, long, DateTime) source = (file.FullName, file.Length, file.LastWriteTimeUtc);
					if (_loaded != null && source == _loadedFrom)
						return _loaded;

					Image image = TryLoad(file.FullName);
					if (image == null)
						continue;

					Replace(image);
					_loadedFrom = source;
					return image;
				}
			}
		}

		Replace(null);
		return null;
	}

	private static void Replace(Image image)
	{
		Image previous = _loaded;
		_loaded = image;
		previous?.Dispose();
	}

	// GDI+ reads an animated GIF's frames from the stream as it plays them, so the stream must live as long as the image:
	// it is never disposed here, and holds only the file's bytes in memory - the file itself is not kept open, so the
	// user can replace it while SM-FAE runs.
	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The stream must outlive the image; see above.")]
	private static Image TryLoad(string path)
	{
		try
		{
			MemoryStream stream = new(File.ReadAllBytes(path));
			try
			{
				return Image.FromStream(stream);
			}
			catch (ArgumentException)
			{
				// Not an image GDI+ can read, whatever its extension says.
				stream.Dispose();
				return null;
			}
		}
		catch (IOException)
		{
			return null;
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}
	}
}

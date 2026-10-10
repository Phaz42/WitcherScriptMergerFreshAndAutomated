using System;
using System.Collections.Generic;

using WitcherScriptMerger.Forms;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.UI;

/// <summary>Why KDiff3 opened for the user - known when it is started (KDiff3.Run).</summary>
internal enum KDiff3Reason
{
	/// <summary>Merging with --auto, and two mods change the same lines: only then does KDiff3 show a window.</summary>
	Conflict,

	/// <summary>No game file to compare with: two panes, every difference a choice - KDiff3 cannot merge on its own.</summary>
	NoGameFile,

	/// <summary>An updated mod merged into an older merge: opened to be checked, because merging on its own would keep
	/// what the old version of the mod changed, even what the new version removed.</summary>
	UpdatedMod,

	/// <summary>"Review each merge" is on: every merge opens. Whether it also has a conflict, only KDiff3 knows.</summary>
	Review
}

/// <summary>
/// What the KDiff3 guide says about the merge in front of the user: the file, and the two sides by the names KDiff3 shows.
/// </summary>
/// <param name="HasGameFile">KDiff3 compares three panes: A the game's own file, B the first side, C the second.
/// Without it, two: A the first side, B the second.</param>
/// <param name="FirstIsMergeSoFar">The first side is the file merged so far from earlier mods, not a mod's own copy.</param>
/// <param name="MergedSoFar">Those earlier mods, by name: the ones the second side may conflict with. The merged mod is
/// no mod a user can uninstall or ask about, so the guide names them instead.</param>
internal sealed record KDiff3GuideContext(string RelativePath, string First, string Second, bool HasGameFile, bool FirstIsMergeSoFar, IReadOnlyList<string> MergedSoFar, KDiff3Reason Reason)
{
	/// <summary>The review guide, for a user who chose to review every merge; otherwise the manual merging guide.</summary>
	internal bool IsReview => Reason == KDiff3Reason.Review;
}

/// <summary>
/// The KDiff3 guide: shown in front of KDiff3 when its window appears, the first time only unless the user asks for
/// every time; it can be kept open beside KDiff3 for the rest of the merge run. The menu opens it any time.
/// Two guides, each remembered on its own: the manual merging guide, and the review guide, for a user who turned on
/// "Review each successful merge in KDiff3". One option, "Show KDiff3 guide", turns both back on.
/// KDiff3.Run reports KDiff3's window from the merge's background thread; everything here runs on the UI thread.
/// </summary>
internal static class KDiff3Guide
{
	private static KDiff3GuideForm _form;

	private static bool IsOpen => _form is { IsDisposed: false };

	private static string ShownSetting(bool review) => review ? "ReviewGuideShown" : "ManualMergeGuideShown";

	private static string EveryTimeSetting(bool review) => review ? "ReviewGuideEveryTime" : "ManualMergeGuideEveryTime";

	internal static bool EveryTime(bool review) => Settings.Get<bool>(EveryTimeSetting(review));

	private static bool WillShow(bool review) => !Settings.Get<bool>(ShownSetting(review)) || EveryTime(review);

	/// <summary>
	/// The options' "Show KDiff3 guide": on while both guides will still show - one guide seen and the other not reads
	/// as off, so ticking it brings back the one that was seen.
	/// </summary>
	internal static bool GuidesOn => WillShow(false) && WillShow(true);

	/// <summary>
	/// Set from the options, for both guides. Only a change does anything - the options save every setting when they
	/// open. Turned on there, both show every time, until turned off here or in a guide.
	/// </summary>
	internal static void SetGuidesOn(bool on)
	{
		if (on == GuidesOn)
			return;

		foreach (bool review in new[] { false, true })
		{
			Settings.Set(ShownSetting(review), true);
			Settings.Set(EveryTimeSetting(review), on);
		}
	}

	/// <summary>KDiff3's main window has appeared: it needs the user. Called from the merge's background thread.</summary>
	internal static void KDiff3Opened(IntPtr kdiff3Window, KDiff3GuideContext context) =>
		MainFrm.BeginInvoke(new Action(() => OnKDiff3Opened(kdiff3Window, context)));

	/// <summary>That KDiff3 has closed. Called from the merge's background thread.</summary>
	internal static void KDiff3Closed() => MainFrm.BeginInvoke(new Action(OnKDiff3Closed));

	/// <summary>The merge run has ended: the guide kept open beside KDiff3 closes with it.</summary>
	internal static void EndRun()
	{
		if (IsOpen && _form.ForKDiff3)
			_form.Close();
	}

	/// <summary>From the menu: the manual merging guide in general terms, centred on SM-FAE.</summary>
	internal static void ShowFromMenu()
	{
		if (IsOpen)
		{
			_form.Activate();
			return;
		}

		Open(null, IntPtr.Zero);
	}

	/// <summary>A guide has left its first, full form - closed, or kept open beside KDiff3: it was seen.</summary>
	internal static void SaveSeen(bool review, bool everyTime)
	{
		Settings.Set(ShownSetting(review), true);
		Settings.Set(EveryTimeSetting(review), everyTime);
		Settings.Save();
	}

	private static void OnKDiff3Opened(IntPtr kdiff3Window, KDiff3GuideContext context)
	{
		// Already open - beside the previous KDiff3 of this run, or from the menu: it moves to this one.
		if (IsOpen)
		{
			_form.AttachTo(kdiff3Window, context);
			return;
		}

		if (WillShow(context.IsReview))
			Open(context, kdiff3Window);
	}

	private static void OnKDiff3Closed()
	{
		if (IsOpen)
			_form.KDiff3Gone();
	}

	private static void Open(KDiff3GuideContext context, IntPtr kdiff3Window)
	{
		_form = new KDiff3GuideForm(context?.IsReview ?? false);
		_form.FormClosed += (sender, e) =>
		{
			KDiff3GuideForm closed = (KDiff3GuideForm)sender;
			if (_form == closed)
				_form = null;
			closed.Dispose();
		};
		ThemeMngr.ApplyThemeOnForm(_form);

		if (kdiff3Window == IntPtr.Zero)
			_form.ShowCentredOn(MainFrm);
		else
			_form.AttachTo(kdiff3Window, context);
	}
}

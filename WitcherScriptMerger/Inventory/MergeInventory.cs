using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.LoadOrder;

namespace WitcherScriptMerger.Inventory;

[XmlRoot]
public class MergeInventory
{
	[XmlElement("Merge")]
	public ObservableCollection<Merge> Merges { get; private set; }

	[XmlIgnore]
	public bool ScriptsChanged { get; private set; }

	[XmlIgnore]
	public bool XmlChanged { get; private set; }

	[XmlIgnore]
	public bool BundleChanged { get; private set; }

	[XmlIgnore]
	public bool HasChanged => ScriptsChanged || XmlChanged || BundleChanged;

	private static XmlSerializer _serializer = new(typeof(MergeInventory));

	internal MergeInventory()
	{
		Merges = [];
		Merges.CollectionChanged += Merges_CollectionChanged;
	}

	internal static MergeInventory Load(string path)
	{
		MergeInventory inventory;
		try
		{
			_serializer = new XmlSerializer(typeof(MergeInventory));
			using (FileStream stream = File.OpenRead(path))
			{
				inventory = (MergeInventory)_serializer.Deserialize(stream);
			}

			AddMissingHashes(inventory);
		}
		catch
		{
			inventory = new MergeInventory();
		}

		inventory.ScriptsChanged = inventory.XmlChanged = inventory.BundleChanged = false;
		return inventory;
	}

	internal static int GetMergeCount(string path)
	{
		MergeInventory inventory;
		try
		{
			_serializer = new XmlSerializer(typeof(MergeInventory));
			using FileStream stream = File.OpenRead(path);
			inventory = (MergeInventory)_serializer.Deserialize(stream);
			return inventory.Merges.Count;
		}
		catch
		{
			return 0;
		}
	}

	private void Merges_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		if ((e.NewItems != null && e.NewItems.Cast<Merge>().Any(merge => merge.Category == Categories.Script)) ||
			(e.OldItems != null && e.OldItems.Cast<Merge>().Any(merge => merge.Category == Categories.Script)))
		{
			ScriptsChanged = true;
		}

		if ((e.NewItems != null && e.NewItems.Cast<Merge>().Any(merge => merge.Category == Categories.Xml)) ||
			(e.OldItems != null && e.OldItems.Cast<Merge>().Any(merge => merge.Category == Categories.Xml)))
		{
			XmlChanged = true;
		}

		if ((e.NewItems != null && e.NewItems.Cast<Merge>().Any(merge => merge.IsBundleContent)) ||
			(e.OldItems != null && e.OldItems.Cast<Merge>().Any(merge => merge.IsBundleContent)))
		{
			BundleChanged = true;
		}
	}

	internal void AddModToMerge(FileMerger.MergeSource source, Merge m)
	{
		string modFilePath =
			m.IsBundleContent
			? source.Bundle.FullName
			: source.TextFile.FullName;

		FileHash existingMod = m.Mods.FirstOrDefault(mod => mod.Name.EqualsIgnoreCase(source.Name));
		if (existingMod != null)
		{
			existingMod.Hash = Tools.Hasher.ComputeHash(modFilePath);
		}
		else
		{
			m.Mods.Add(
				new FileHash
				{
					Hash = Tools.Hasher.ComputeHash(modFilePath),
					Name = source.Name
				});
		}

		if (m.Category == Categories.Script)
			ScriptsChanged = true;
		else if (m.Category == Categories.Xml)
			XmlChanged = true;
		else if (m.IsBundleContent)
			BundleChanged = true;
	}

	internal void Save()
	{
		if (_serializer == null)
			return;

		using StreamWriter writer = new(Paths.Inventory);
		_serializer.Serialize(writer, this);
	}

	internal bool HasResolvedConflict(ModFile conflict)
	{
		if (conflict == null)
			throw new ArgumentNullException(nameof(conflict));
		Merge merge = Merges.FirstOrDefault(mrg => mrg.RelativePath.EqualsIgnoreCase(conflict.RelativePath));
		return merge != null
			&& !conflict.Mods.Any(mod => !mod.Name.EqualsIgnoreCase(merge.MergedModName) && !merge.ContainsMod(mod.Name))
			&& !merge.Mods.Any(mod => new LoadOrderComparer().Compare(merge.MergedModName, mod.Name) > 0)
			&& merge.Mods.All(mod => mod.Hash == Tools.Hasher.ComputeHash(merge.GetModFile(mod.Name)));
	}

	internal Merge GetMergeByRelativePath(string relativePath) => Merges.FirstOrDefault(m => m.RelativePath.EqualsIgnoreCase(relativePath));

	// Adds file hashes to old inventories that don't have them
	private static void AddMissingHashes(MergeInventory inventory)
	{
		bool anyMissing = false;

		foreach (Merge merge in inventory.Merges)
		{
			foreach (FileHash mod in merge.Mods)
			{
				if (mod.Hash == null)
				{
					anyMissing = true;
					mod.Hash = Tools.Hasher.ComputeHash(merge.GetModFile(mod.Name));
				}
			}
		}

		if (anyMissing)
			inventory.Save();
	}
}

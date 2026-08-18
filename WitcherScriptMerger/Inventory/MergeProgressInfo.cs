using System.ComponentModel;

namespace WitcherScriptMerger.Inventory;

internal partial class MergeProgressInfo : INotifyPropertyChanged
{
	private string _currentAction;
	internal string CurrentAction
	{
		get => _currentAction;
		set => Set(ref _currentAction, value);
	}

	private string _currentPhase;
	internal string CurrentPhase
	{
		get => _currentPhase;
		set => Set(ref _currentPhase, value);
	}

	private int _currentMergeNum;
	internal int CurrentMergeNum
	{
		get => _currentMergeNum;
		set
		{
			_currentMergeNum = value;
			UpdatePhase();
		}
	}

	private int _totalMergeCount;
	internal int TotalMergeCount
	{
		get => _totalMergeCount;
		set
		{
			_totalMergeCount = value;
			UpdatePhase();
		}
	}

	private string _currentFileName;
	internal string CurrentFileName
	{
		get => _currentFileName;
		set
		{
			_currentFileName = value;
			UpdatePhase();
		}
	}

	private int _currentFileNum;
	internal int CurrentFileNum
	{
		get => _currentFileNum;
		set
		{
			_currentFileNum = value;
			UpdatePhase();
		}
	}

	private int _totalFileCount;
	internal int TotalFileCount
	{
		get => _totalFileCount;
		set
		{
			_totalFileCount = value;
			UpdatePhase();
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged() => PropertyChanged?.Invoke(this, null);

	private void Set<T>(ref T property, T value)
	{
		property = value;
		OnPropertyChanged();
	}

	private void UpdatePhase()
	{
		CurrentPhase =
			"Resolving mod conflict" +
			(
				TotalMergeCount > 1
				? $" {CurrentMergeNum} of {TotalMergeCount}" : ""
			) +
			"\nFile" +
			(
				TotalFileCount > 1 && TotalFileCount != TotalMergeCount
				? $" {CurrentFileNum} of {TotalFileCount}" : ""
			) +
			$": {CurrentFileName}";
	}
}

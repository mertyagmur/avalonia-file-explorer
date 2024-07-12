using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileExplorer.Services;
using System.Collections.ObjectModel;
using System.IO;
using System;
using System.Linq;
using System.Windows.Input;
using FileExplorer.Models;

namespace FileExplorer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
	[ObservableProperty]
	private string _currentPath = "C:\\";

	[ObservableProperty]
	private string _nameFilter = "";

	[ObservableProperty]
	private string _extensionFilter = "";

	[ObservableProperty]
	private bool _hideDirectories;

	[ObservableProperty]
	private bool _hideFiles;

	[ObservableProperty]
	private ObservableCollection<FileSystemItem> _items = new();

	[ObservableProperty]
	private ObservableCollection<string> _logs = new();

	public ICommand NavigateCommand { get; }
	public ICommand GoBackCommand { get; }

	public MainViewModel()
	{
		NavigateCommand = new RelayCommand<string>(Navigate);
		GoBackCommand = new RelayCommand(GoBack);

		PropertyChanged += (_, args) =>
		{
			if (args.PropertyName == nameof(NameFilter) ||
				args.PropertyName == nameof(ExtensionFilter) ||
				args.PropertyName == nameof(HideDirectories) ||
				args.PropertyName == nameof(HideFiles))
			{
				LoadItems();
			}
		};

		LoadItems();
	}

	[RelayCommand]
	private void ItemDoubleTapped(FileSystemItem item)
	{
		if (item.IsDirectory)
		{
			Navigate(item.FullPath);
		}
	}

	partial void OnCurrentPathChanged(string value)
	{
		if (Directory.Exists(value))
		{
			LoadItems();
		}
	}

	private void Navigate(string path)
	{
		if (Directory.Exists(path))
		{
			CurrentPath = path;
			LoadItems();
		}
	}

	private void GoBack()
	{
		var parent = Directory.GetParent(CurrentPath);
		if (parent != null)
		{
			CurrentPath = parent.FullName;
			LoadItems();
		}
	}

	private void LoadItems()
	{
		Items.Clear();
		Logs.Clear();

		FileSystemVisitor visitor;

		bool isFilterApplied = !string.IsNullOrEmpty(NameFilter) ||
							   !string.IsNullOrEmpty(ExtensionFilter) ||
							   HideDirectories || HideFiles;

		// Option 1: Use only GUI filters
		if (isFilterApplied)
		{
			visitor = new FileSystemVisitor(CurrentPath, FilterItem);
		}
		else
		{
			// No filters applied
			visitor = new FileSystemVisitor(CurrentPath);
		}

		// Option 2: Use GUI filters combined with a custom filter
		// Uncomment the following lines and comment out the option 1 lines to use this option

		// Custom filter example 1
		//Func<FileSystemInfo, bool> customFilter = (item) => item.LastWriteTime > DateTime.Now.AddDays(-7);

		// Custom filter example 2
		//Func<FileSystemInfo, bool> customFilter = (item) => (item is FileInfo f && f.Length > 1_000);

		//visitor = new FileSystemVisitor(CurrentPath, FilterItem, customFilter);


		visitor.Start += (_, _) => AddLog("Search started");
		visitor.Finish += (_, _) => AddLog("Search finished");
		visitor.FileFound += (_, args) => AddLog($"File found: {args.Item.Name}");
		visitor.DirectoryFound += (_, args) => AddLog($"Directory found: {args.Item.Name}");
		visitor.FilteredFileFound += (_, args) => AddLog($"Filtered file found: {args.Item.Name}");
		visitor.FilteredDirectoryFound += (_, args) => AddLog($"Filtered directory found: {args.Item.Name}");
		visitor.ErrorOccurred += (_, args) => AddLog($"Error accessing item: {args.GetException().Message}");

		foreach (var item in visitor.GetFileSystemInfos().OrderBy(i => i is DirectoryInfo ? 0 : 1))
		{
			if (item is DirectoryInfo dir)
			{
				Items.Add(new FileSystemItem { Name = dir.Name, IsDirectory = true, FullPath = dir.FullName });
			}
			else if (item is FileInfo file)
			{
				Items.Add(new FileSystemItem { Name = file.Name, IsDirectory = false, FullPath = file.FullName });
			}
		}
	}

	private bool FilterItem(FileSystemInfo item)
	{
		if (item is DirectoryInfo && HideDirectories) return false;
		if (item is FileInfo && HideFiles) return false;

		if (!string.IsNullOrEmpty(NameFilter) && !item.Name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase))
			return false;

		if (!string.IsNullOrEmpty(ExtensionFilter) && item is FileInfo file)
		{
			if (!file.Extension.Equals(ExtensionFilter, StringComparison.OrdinalIgnoreCase))
				return false;
		}

		return true;
	}

	private void AddLog(string message)
	{
		Logs.Insert(0, $"{DateTime.Now:HH:mm:ss} - {message}");
	}
}

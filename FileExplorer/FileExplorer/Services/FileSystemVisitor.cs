using System;
using System.Collections.Generic;
using System.IO;

namespace FileExplorer.Services;

public class FileSystemVisitor
{
	private readonly string _rootPath;
	private readonly Func<FileSystemInfo, bool> _filter;
	private readonly bool _isFilterApplied;

	public event EventHandler<EventArgs> Start;
	public event EventHandler<EventArgs> Finish;
	public event EventHandler<FileSystemEventArgs> FileFound;
	public event EventHandler<FileSystemEventArgs> DirectoryFound;
	public event EventHandler<FileSystemEventArgs> FilteredFileFound;
	public event EventHandler<FileSystemEventArgs> FilteredDirectoryFound;
	public event EventHandler<ErrorEventArgs> ErrorOccurred;

	// Constructor with no filter
	public FileSystemVisitor(string rootPath)
	{
		_rootPath = rootPath;
		_filter = _ => true;
		_isFilterApplied = false;
	}

	// Constructor with GUI filter
	public FileSystemVisitor(string rootPath, Func<FileSystemInfo, bool> guiFilter)
	{
		_rootPath = rootPath;
		_filter = guiFilter;
		_isFilterApplied = true;
	}

	// Overloaded constructor for custom filter combined with GUI filter
	public FileSystemVisitor(string rootPath, Func<FileSystemInfo, bool> guiFilter, Func<FileSystemInfo, bool> customFilter)
	{
		_rootPath = rootPath;
		_filter = item => guiFilter(item) && customFilter(item);
		_isFilterApplied = true;
	}

	public IEnumerable<FileSystemInfo> GetFileSystemInfos()
	{
		Start?.Invoke(this, EventArgs.Empty);

		foreach (var item in TraverseFileSystem(_rootPath))
		{
			yield return item;
		}

		Finish?.Invoke(this, EventArgs.Empty);
	}
	protected virtual IEnumerable<FileSystemInfo> GetFileSystemInfosInternal(string path)
	{
		return new DirectoryInfo(path).EnumerateFileSystemInfos();
	}

	protected virtual IEnumerable<FileSystemInfo> TraverseFileSystem(string path)
	{
		IEnumerable<FileSystemInfo> items;

		try
		{
			items = GetFileSystemInfosInternal(path);
		}
		catch (Exception ex)
		{
			ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
			yield break;
		}

		foreach (var item in items)
		{
			var eventArgs = new FileSystemEventArgs(item);

			if (_isFilterApplied)
			{
				if (_filter(item))
				{
					if (item is DirectoryInfo)
					{
						FilteredDirectoryFound?.Invoke(this, eventArgs);
					}
					else
					{
						FilteredFileFound?.Invoke(this, eventArgs);
					}

					if (!eventArgs.ExcludeFromResults)
					{
						yield return item;
					}
				}
			}
			else
			{
				if (item is DirectoryInfo)
				{
					DirectoryFound?.Invoke(this, eventArgs);
				}
				else
				{
					FileFound?.Invoke(this, eventArgs);
				}

				if (!eventArgs.ExcludeFromResults)
				{
					yield return item;
				}
			}

			if (eventArgs.Cancel)
			{
				yield break;
			}
		}
	}
}
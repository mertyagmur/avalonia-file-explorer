using System;
using System.IO;

namespace FileExplorer.Services;

public class FileSystemEventArgs : EventArgs
{
	public FileSystemInfo Item { get; }
	public bool ExcludeFromResults { get; set; }
	public bool Cancel { get; set; }

	public FileSystemEventArgs(FileSystemInfo item)
	{
		Item = item;
	}
}

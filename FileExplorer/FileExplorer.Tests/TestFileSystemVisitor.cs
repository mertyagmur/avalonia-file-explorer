using FileExplorer.Services;

namespace FileExplorer.Tests;

class TestFileSystemVisitor : FileSystemVisitor
{
	private readonly IFileSystem _fileSystem;

	public TestFileSystemVisitor(string rootPath, IFileSystem fileSystem) : base(rootPath)
	{
		_fileSystem = fileSystem;
	}

	public TestFileSystemVisitor(string rootPath, IFileSystem fileSystem, Func<FileSystemInfo, bool> guiFilter)
		: base(rootPath, guiFilter)
	{
		_fileSystem = fileSystem;
	}

	public TestFileSystemVisitor(string rootPath, IFileSystem fileSystem, Func<FileSystemInfo, bool> guiFilter, Func<FileSystemInfo, bool> customFilter)
		: base(rootPath, guiFilter, customFilter)
	{
		_fileSystem = fileSystem;
	}

	protected override IEnumerable<FileSystemInfo> GetFileSystemInfosInternal(string path)
	{
		return _fileSystem.GetFileSystemInfos(path);
	}
}

using Moq;
using FileExplorer.Services;

namespace FileExplorer.Tests;

public class FileSystemVisitorTests
{
	private Mock<IFileSystem> _mockFileSystem;

	public FileSystemVisitorTests()
	{
		_mockFileSystem = new Mock<IFileSystem>();
	}

	[Fact]
	public void Test_DefaultConstructor()
	{
		var visitor = new FileSystemVisitor("C:\\");
		Assert.NotNull(visitor);
	}

	[Fact]
	public void Test_ConstructorWithGuiFilter()
	{
		Func<FileSystemInfo, bool> guiFilter = (info) => info.Name.Contains("A");
		var visitor = new FileSystemVisitor("C:\\", guiFilter);
		Assert.NotNull(visitor);
	}

	[Fact]
	public void Test_ConstructorWithGuiAndCustomFilter()
	{
		Func<FileSystemInfo, bool> guiFilter = (info) => info.Name.Contains("A");
		Func<FileSystemInfo, bool> customFilter = (info) => info.LastWriteTime > DateTime.Now.AddDays(-7);
		var visitor = new FileSystemVisitor("C:\\", guiFilter, customFilter);
		Assert.NotNull(visitor);
	}

	[Fact]
	public void Test_GetFileSystemInfos_ReturnsAllItems()
	{
		var mockItems = new List<FileSystemInfo>
		{
			new FileInfo("file1.txt"),
			new DirectoryInfo("dir1"),
		};

		_mockFileSystem.Setup(fs => fs.GetFileSystemInfos(It.IsAny<string>()))
			.Returns(mockItems);

		var visitor = new TestFileSystemVisitor("C:\\", _mockFileSystem.Object);
		var result = visitor.GetFileSystemInfos().ToList();

		Assert.Equal(2, result.Count);
		Assert.Contains(result, item => item.Name == "file1.txt");
		Assert.Contains(result, item => item.Name == "dir1");
	}

	[Fact]
	public void Test_GetFileSystemInfos_AppliesGuiFilter()
	{
		var mockItems = new List<FileSystemInfo>
		{
			new FileInfo("Afile.txt"),
			new FileInfo("Bfile.txt"),
			new DirectoryInfo("Adir"),
			new DirectoryInfo("Bdir")
		};

		_mockFileSystem.Setup(fs => fs.GetFileSystemInfos(It.IsAny<string>()))
			.Returns(mockItems);

		Func<FileSystemInfo, bool> guiFilter = (info) => info.Name.Contains("A");
		var visitor = new TestFileSystemVisitor("C:\\", _mockFileSystem.Object, guiFilter);
		var result = visitor.GetFileSystemInfos().ToList();

		Assert.Equal(2, result.Count);
		Assert.Contains(result, item => item.Name == "Afile.txt");
		Assert.Contains(result, item => item.Name == "Adir");
	}

	[Fact]
	public void Test_StartEvent_IsRaised()
	{
		_mockFileSystem.Setup(fs => fs.GetFileSystemInfos(It.IsAny<string>()))
			.Returns(new List<FileSystemInfo>());

		var visitor = new TestFileSystemVisitor("C:\\", _mockFileSystem.Object);
		var eventRaised = false;
		visitor.Start += (sender, args) => eventRaised = true;

		visitor.GetFileSystemInfos().ToList();

		Assert.True(eventRaised);
	}

	[Fact]
	public void Test_FinishEvent_IsRaised()
	{
		_mockFileSystem.Setup(fs => fs.GetFileSystemInfos(It.IsAny<string>()))
			.Returns(new List<FileSystemInfo>());

		var visitor = new TestFileSystemVisitor("C:\\", _mockFileSystem.Object);
		var eventRaised = false;
		visitor.Finish += (sender, args) => eventRaised = true;

		visitor.GetFileSystemInfos().ToList();

		Assert.True(eventRaised);
	}

	[Fact]
	public void Test_FileFoundEvent_IsRaised()
	{
		var mockItems = new List<FileSystemInfo>
		{
			new FileInfo("file1.txt")
		};

		_mockFileSystem.Setup(fs => fs.GetFileSystemInfos(It.IsAny<string>()))
			.Returns(mockItems);

		var visitor = new TestFileSystemVisitor("C:\\", _mockFileSystem.Object);
		var eventRaised = false;
		visitor.FileFound += (sender, args) => eventRaised = true;

		visitor.GetFileSystemInfos().ToList();

		Assert.True(eventRaised);
	}

	[Fact]
	public void Test_ErrorOccurredEvent_IsRaised()
	{
		_mockFileSystem.Setup(fs => fs.GetFileSystemInfos(It.IsAny<string>()))
			.Throws(new UnauthorizedAccessException());

		var visitor = new TestFileSystemVisitor("C:\\", _mockFileSystem.Object);
		var eventRaised = false;
		visitor.ErrorOccurred += (sender, args) => eventRaised = true;

		visitor.GetFileSystemInfos().ToList();

		Assert.True(eventRaised);
	}

	[Fact]
	public void Test_Cancellation_StopsTraversal()
	{
		var mockItems = new List<FileSystemInfo>
		{
			new FileInfo("file1.txt"),
			new FileInfo("file2.txt"),
			new FileInfo("file3.txt")
		};

		_mockFileSystem.Setup(fs => fs.GetFileSystemInfos(It.IsAny<string>()))
			.Returns(mockItems);

		var visitor = new TestFileSystemVisitor("C:\\", _mockFileSystem.Object);
		visitor.FileFound += (sender, args) =>
		{
			if (args.Item.Name == "file2.txt")
				args.Cancel = true;
		};

		var result = visitor.GetFileSystemInfos().ToList();

		Assert.Equal(2, result.Count);
		Assert.Contains(result, item => item.Name == "file1.txt");
		Assert.Contains(result, item => item.Name == "file2.txt");
		Assert.DoesNotContain(result, item => item.Name == "file3.txt");
	}

	[Fact]
	public void Test_ExcludeFromResults_RemovesItem()
	{
		var mockItems = new List<FileSystemInfo>
		{
			new FileInfo("file1.txt"),
			new FileInfo("file2.txt"),
			new FileInfo("file3.txt")
		};

		_mockFileSystem.Setup(fs => fs.GetFileSystemInfos(It.IsAny<string>()))
			.Returns(mockItems);

		var visitor = new TestFileSystemVisitor("C:\\", _mockFileSystem.Object);
		visitor.FileFound += (sender, args) =>
		{
			if (args.Item.Name == "file2.txt")
				args.ExcludeFromResults = true;
		};

		var result = visitor.GetFileSystemInfos().ToList();

		Assert.Equal(2, result.Count);
		Assert.Contains(result, item => item.Name == "file1.txt");
		Assert.DoesNotContain(result, item => item.Name == "file2.txt");
		Assert.Contains(result, item => item.Name == "file3.txt");
	}
}
public interface IFileSystem
{
	IEnumerable<FileSystemInfo> GetFileSystemInfos(string path);
}
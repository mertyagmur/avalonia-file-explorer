
# Avalonia File Explorer

This is a simple and lightweight file system explorer built with Avalonia UI and .NET. It allows users to browse the file system and apply filters.


## Features

- Browse local file system
- Apply filters to displayed files and folders
- Apply custom filtering algorithm
- Custom file system traversal with event-based logging
- MVVM architecture
- Responsive UI with asynchronous operations


## Prerequisites

- .NET 6.0 SDK or later
- An IDE that supports Avalonia development (e.g., Visual Studio, JetBrains Rider, VS Code with C# extension)
- Windows operating system
## Installation

- Clone the repository

```bash
  git clone https://github.com/yourusername/AvanoniaFileExplorer.git
```
- Open the solution in your preferred IDE
- Restore NuGet packages and build the solution
- Set the desktop project as the startup project
- Run the application
## Screenshots

![App Screenshot](/../<main>/screenshots/1.png?raw=true)


## Usage/Examples

### Adding Custom Filtering Algorithm

You can create your own custom filters like in the following examples. You can then use the overloaded FileSystemVisitor constructor to apply your custom filter.
```csharp
// Custom filter example 1
Func<FileSystemInfo, bool> customFilter1 = (item) => item.LastWriteTime > DateTime.Now.AddDays(-7);

// Custom filter example 2
Func<FileSystemInfo, bool> customFilter2 = (item) => (item is FileInfo f && f.Length > 1_000);

visitor = new FileSystemVisitor(CurrentPath, FilterItem, customFilter1);
```


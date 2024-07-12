using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models;

public class FileSystemItem
{
	public string Name { get; set; }
	public bool IsDirectory { get; set; }
	public string FullPath { get; set; }
}

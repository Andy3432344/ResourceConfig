using System;
using System.Text;

namespace ModuleResourceProvider.Test;

public class TestFileSystem : IFileSystem
{
	private readonly Dictionary<string, TestFileStream> files = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, List<string>> childDirectories = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, List<string>> childFiles = new(StringComparer.OrdinalIgnoreCase);
	public TestFileSystem(Dictionary<string, TestFileStream> testFiles)
	{
		foreach (var pair in testFiles)
		{
			var dir = Path.GetDirectoryName(pair.Key);

			while (!string.IsNullOrEmpty(dir))
			{
				string child = dir;
				dir = Path.GetDirectoryName(pair.Key);
				if (!string.IsNullOrEmpty(dir))
				{
					if (!childDirectories.TryGetValue(dir, out var subs))
						childDirectories[dir] = subs = new();

					subs.Add(child);

				}
			}
			dir = Path.GetDirectoryName(pair.Key);
			if (!string.IsNullOrEmpty(dir) && !childFiles.TryGetValue(dir, out var children))
				childFiles[dir] = children = new();

			files.Add(pair.Key, pair.Value);
		}
	}

	public bool Exists(string path)
	{
		return files.ContainsKey(path);
	}

	public IEnumerable<string> GetDirectories(string path)
	{
		if (childDirectories.TryGetValue(path, out var subs))
		{
			return subs;
		}
		return [];
	}

	public IEnumerable<string> GetDirectoryContents(string path)
	{
		foreach (var dir in GetDirectories(path))
			yield return dir;

		foreach (var file in GetFiles(path))
			yield return file;
	}

	public Stream GetFileAtPath(string path)
	{

		if (files.TryGetValue(path, out var stream))
			return stream;

		return new TestFileStream([]);
	}

	public IEnumerable<string> GetFiles(string path)
	{
		if (childFiles.TryGetValue(path, out var list))
			return list;
		return [];
	}

	public IFileSystem GetNewBase(string path)
	{
		return this;
	}

	public string ReadAllText(string path)
	{
		var file = GetFileAtPath(path);
		byte[] data = new byte[file.Length];
		file.Read(data, 0, (int)file.Length);
		return UTF8Encoding.UTF8.GetString(data);
	}
}

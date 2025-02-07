namespace ModuleResourceProvider;

public interface IFileSystem
{
	IFileSystem GetNewBase(string path);
    Stream GetFileAtPath(string path);
    string ReadAllText(string path);
    IEnumerable<string> GetFiles(string path);
    IEnumerable<string> GetDirectories(string path);
    IEnumerable<string> GetDirectoryContents(string path);
}


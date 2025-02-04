using System.Text;
using CatConfig;
using CatConfig.CclParser;
using CatConfig.CclUnit;
namespace ModuleResourceProvider;

public class ModuleProvider : IResourceProvider
{
    private const string mod = "mod";
    private NoValue noValue = new();


    private readonly IUnitRecord library;
    private readonly IFileSystem fs;

    public ModuleProvider(string path, IFileSystem fs)
    {
        string ccl = GetFileContent(fs, path);
        var parser = Parser.FromContent("", ccl);
        var unit = parser.ParseContent("", ccl);
        var config = unit as IUnitRecord
            ?? new NoRecord();

        ResourceName = (config["module"] as IUnitValue)?.Value ?? "NoModule";
        library = GetModuleLibrary(config);

        this.fs = fs;
    }


    public string DataFormat => mod;
    public string ResourceName { get; }

    private IUnitRecord GetModuleLibrary(IUnitRecord config)
    {
        var imports = config["imports"] as IUnitArray;
        if (imports == null)
            imports = new NoArray();

        var exports = config["exports"] as IUnitArray;
        if (exports == null)
            exports = new NoArray();

        //do things with import/export items....

        return config["library"] as IUnitRecord ??
            new NoRecord();
    }

    private static string GetFileContent(IFileSystem fs, string path)
    {
        var file = fs.GetFileAtPath(path);
        byte[] data = new byte[file.Length];

        int length = (int)long.Clamp(file.Length, 0, Int32.MaxValue);
        file.Read(data, 0, length);
        string ccl = UTF8Encoding.UTF8.GetString(data);
        return ccl;
    }

    public IUnit GetResource(int id, UnitPath path)
    {
        string name = path[0];

        var func = library[name] as IDelayedUnit;
        if (func == null)
            return noValue;


        return library[func](path[1..]);
    }
}

using System.Text;
using CatConfig;
using CatConfig.CclParser;
using CatConfig.CclUnit;
namespace ModuleResourceProvider;

public class ModuleProvider : IResourceProvider
{
    private const string mod = "mod";
    private NoValue noValue = new();

    private Dictionary<string, string> imports = new(StringComparer.OrdinalIgnoreCase);


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

        var import = config["import"] as IUnitRecord;
        if (import == null)
            import = new NoRecord();

        var exports = config["exports"] as IUnitArray;
        if (exports == null)
            exports = new NoArray();

        //do things with import/export items....

        foreach (var field in import.FieldNames)
        {
            var imported = import[field] as IUnitValue;
            imports.TryAdd(field, imported?.Value ?? $"No-{field}-Value");
        }


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
        string[] args = [];
        if (path.Length > 1)
            args = path[1..];

        var unit = library[name];

        if (unit is IDelayedUnit func)
            return library[func](args);

        return unit;
    }
}

using System.Reflection;
using System.Text;
using CatConfig;
using CatConfig.CclParser;
using CatConfig.CclUnit;
using Microsoft.Extensions.DependencyModel;
namespace ModuleResourceProvider;
public class ModuleProcessor : IDelayedProcessor
{
    private const string mod = "mod";
    private const string res = "res";
    private const string modules = "modules";
    private readonly IUnitRecord config;
    private readonly IFileSystem fs;

    public string Name => mod;
    public string ProtocolSchema => res;

    public ModuleProcessor(IFileSystem fs)
    {
        string ccl = GetFileContent(fs, $"{mod}.{mod}");
        var parser = Parser.FromContent("", ccl);
        var unit = parser.ParseContent("", ccl);
        config = unit as IUnitRecord
            ?? new NoRecord();
        this.fs = fs;
        setup();
        Constructor.RegisterProcessor(this);
    }

    private void setup()
    {
        var modes = config[modules];
        List<string> mods = new();

        if (modes is IUnitArray m)
            mods.AddRange(m.Elements.Select(e => (e as IUnitValue)?.Value ?? ""));
        else if (modes is IUnitValue unit)
            mods.Add(unit.Value);


        foreach (var mod in mods)
        {
            var module = new ModuleProvider(mod, fs);
            ResourceEngine.RegisterProvider(module);
        }



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

    public IUnit ResolveDelayedUnit(int id, string name, UnitPath path)
    {
        string resource = path[0];

        return ResourceEngine.GetResource(id, mod, resource, path[1..]);
    }
}
public class ExternalProcessor : IDelayedProcessor
{
    private readonly Dictionary<string, Dictionary<string, Func<string[], string>>> functions = new(StringComparer.OrdinalIgnoreCase);

    public ExternalProcessor(IFileSystem fs)
    {

        foreach (var file in fs.GetFiles(Name))
        {
            if (Path.GetExtension(file).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                string path = Path.GetFullPath(file);
                var assembly = Assembly.LoadFrom(path);
            }
        }



        Constructor.RegisterProcessor(this);
    }

    public string Name => "exo";
    public string ProtocolSchema => "res";

    private void AddExternal(string name, params (string Name, Func<string[], string> Function)[] modFunctions)
    {
        if (!functions.TryGetValue(name, out var module))
            functions[name] = module = new(StringComparer.OrdinalIgnoreCase);

        foreach (var f in modFunctions)
            module.TryAdd(f.Name, f.Function);


    }

    public IUnit ResolveDelayedUnit(int id, string name, UnitPath path)
    {
        string key = path[0];
        string function = path[1];
        var args = path[2..];

        if (functions.TryGetValue(key, out var f))
            if (f.TryGetValue(function, out var func))
                return new UnitValue(id, func(args));

        return new NoValue();
    }
}

public class Types
{
    private IEnumerable<Type> DiscoverAllTypes(IEnumerable<string> assemblyPrefixesToInclude)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
            return [];

        var dependencyModel =
        DependencyContext.Load(entryAssembly);
        if (dependencyModel == null)
            return [];

        var projectReferenceAssemblies =
        dependencyModel.RuntimeLibraries?
        .Where(_ => _.Type.Equals("project"))
        .Select(_ => Assembly.Load(_.Name)).ToArray()
        ?? [];

        var assemblies = (dependencyModel.RuntimeLibraries ?? [])
        .Where(_ =>
        _.RuntimeAssemblyGroups.Count > 0 &&
        assemblyPrefixesToInclude.Any(assem =>
        _.Name.StartsWith(assem)))
        .Select(_ =>
        {
            try
            {
                return Assembly.Load(_.Name);

            }
            catch
            {
                return null;
            }
        })
        .Where(_ => _ is not null)
        .Distinct()
        .ToList();

        return assemblies.Concat(projectReferenceAssemblies).SelectMany(_ => _?.GetTypes() ?? []);
    }
}
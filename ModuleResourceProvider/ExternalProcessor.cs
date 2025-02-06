using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Xml.Schema;
using CatConfig;
using CatConfig.CclParser;
using CatConfig.CclUnit;
using ResourceConfig;
namespace ModuleResourceProvider;

public class ExternalProcessor : IDelayedProcessor
{
    private const string mod = "mod";

    private readonly Dictionary<string, Dictionary<string, Func<string[], string>>> functions = new(StringComparer.OrdinalIgnoreCase);

    public ExternalProcessor(IFileSystem fs)
    {
        string moduleFile = fs.ReadAllText($"{Name}.{mod}");
        var parser = Parser.FromContent("", moduleFile);
        var modRecord = parser.ParseContent("", moduleFile) as IUnitRecord;
        if (modRecord != null)
        {
            var modules = modRecord["modules"];
            List<IUnit> units = new();
            if (modules is IUnitArray a)
                units = a.Elements.ToList();
            else
                units.Add(modules);

            var allowed = units.Select(u => u as IUnitValue).Where(_ => _ != null).Select(v => v!.Value);

            List<Type> externals = new();
            foreach (var plugin in fs.GetDirectories(Name))
                if (allowed.Contains( new DirectoryInfo(Path.GetFullPath(plugin)).Name , StringComparer.OrdinalIgnoreCase))
                    foreach (var file in fs.GetFiles(plugin))
                        if (Path.GetExtension(file).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            var assembly = Assembly.LoadFrom(Path.GetFullPath(file));
                            externals.AddRange(assembly.GetExportedTypes()
                            .Where(t => allowed.Any(a => t.Name.EndsWith(a, StringComparison.OrdinalIgnoreCase) && t.Name.StartsWith("ResExtern"))));

                        }



            SetupFunctions(externals, functions);

            Constructor.RegisterProcessor(this);
        }
    }

    private static void SetupFunctions(IEnumerable<Type> externals, Dictionary<string, Dictionary<string, Func<string[], string>>> functions)
    {
        foreach (var mod in externals)
        {
            try
            {
                object? plugin = Activator.CreateInstance(mod);
                if (plugin == null)
                    continue;

                if (!functions.TryGetValue(mod.Name, out var funcs))
                    functions[mod.Name] = funcs = new(StringComparer.OrdinalIgnoreCase);

                foreach (var exo in mod.GetMembers().Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(ExternAttribute))))
                    funcs.Add(exo.Name, args => mod.GetMethod(exo.Name)?.Invoke(plugin, args)?.ToString() ?? "");
            }
            catch { }
        }
    }

    public string Name => "exo";
    public string ProtocolSchema => "res";

    public IUnit ResolveDelayedUnit(int id, string name, UnitPath path)
    {
        string key = "ResExtern" + path[0];
        string function = path[1];
        var args = path[2..];

        if (functions.TryGetValue(key, out var f))
            if (f.TryGetValue(function, out var func))
                return new UnitValue(id, func(args));

        return new NoValue();
    }
}


using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Xml.Schema;
using CatConfig;
using CatConfig.CclParser;
using CatConfig.CclUnit;
using ResourceConfig;
namespace ModuleResourceProvider;

public class ExternalResourceProvider : IResourceProvider
{
	private const string mod = nameof(mod);

	private readonly Dictionary<string, Dictionary<string, Func<string[], string>>> functions = new(StringComparer.OrdinalIgnoreCase);

	public ExternalResourceProvider(IFileSystem fs)
	{
		string moduleFile = fs.ReadAllText($"{ResourceName}.{mod}");
		var parser = Parser.FromContent("", moduleFile);
		var modRecord = parser.ParseContent("module", moduleFile) as IUnitRecord;

		if (modRecord != null)
			SetupExternalResources(fs, modRecord);

		ResourceEngine.RegisterProvider(this);
	}

	private void SetupExternalResources(IFileSystem fs, IUnitRecord modRecord)
	{
		var modules = modRecord["exports"];
		List<IUnit> units = new();
		if (modules is IUnitArray a)
			units = a.Elements.ToList();
		else
			units.Add(modules);

		var allowed = units.Select(u => u as IUnitValue).Where(_ => _ != null).Select(v => v!.Value);

		List<Type> ext = new();
		foreach (var plugin in fs.GetDirectories(ResourceName))
			if (allowed.Contains(new DirectoryInfo(Path.GetFullPath(plugin)).Name, StringComparer.OrdinalIgnoreCase))
				foreach (var file in fs.GetFiles(plugin))
					if (Path.GetExtension(file).Equals(".dll", StringComparison.OrdinalIgnoreCase))
					{
						var assembly = Assembly.LoadFrom(Path.GetFullPath(file));
						ext.AddRange(assembly.GetExportedTypes()
						.Where(t => allowed.Any(a => t.Name.EndsWith(a, StringComparison.OrdinalIgnoreCase) && t.Name.StartsWith("ResExtern"))));
					}

		SetupFunctions(ext, functions);
	}

	public IUnit GetResource(int id, UnitPath path)
	{
		string key = "ResExtern" + path[0];
		string function = path[1];
		var args = path[2..];

		if (functions.TryGetValue(key, out var f))
			if (f.TryGetValue(function, out var func))
				return new UnitValue(id, func(args));

		return new NoValue();

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

				string[] allowedAttributes = ["ExternAttribute", "Extern"];

				foreach (var ext in mod.GetMembers().Where(m => m.CustomAttributes.Any(a => allowedAttributes.Contains(a.AttributeType.Name,StringComparer.OrdinalIgnoreCase))))
					funcs.Add(ext.Name, args => mod.GetMethod(ext.Name)?.Invoke(plugin, args)?.ToString() ?? "");
			}
			catch { }
		}
	}


	public string DataFormat => "ext";
	public string ResourceName => "dotnet";

}


using System;
using System.Data;
using System.Text;
using CatConfig;
using CatConfig.CclParser;
using CatConfig.CclUnit;
using static ModuleResourceProvider.ModuleProviderHelpers;

namespace ModuleResourceProvider;

public class ModuleResourceProvider : IResourceProvider, IModuleProvider
{
	private const string mod = nameof(mod);
	private const string NoType = nameof(NoType);
	private readonly Dictionary<string, string> imports;
	private readonly string[] args;
	private readonly IUnitRecord library;
	private readonly IUnitRecord config;
	private readonly string resourceType;

	public ModuleResourceProvider(string path, IFileSystem fs)
	{
		string ccl = GetFileContent(fs, path);
		var parser = Parser.FromContent("", ccl);
		var unit = parser.ParseContent("", ccl);



		config = unit as IUnitRecord
			?? new NoRecord();

		ResourceName = (config["module"] as IUnitValue)?.Value ?? "NoModule";
		resourceType = (config["type"] as IUnitValue)?.Value ?? NoType;

		UnitText = "Library";
		var args = GetImports(config).ToDictionary(StringComparer.OrdinalIgnoreCase);
		this.args = args.Where(a => a.Value == null).Select(a => a.Key).ToArray();

		imports = args.Select(a => (a.Key, a.Value ?? "")).ToDictionary(StringComparer.OrdinalIgnoreCase);
		Exports = GetExports(config);
		library = config["library"] as IUnitRecord ??
			new NoRecord();

		ResourceEngine.RegisterProvider(this);
	}



	public string DataFormat => mod;
	public string ResourceName { get; }
	public string UnitText { get; }
	public string[] Imports => imports.Keys.ToArray();
	public string[] Exports { get; }

	public string this[string import]
	{
		get
		{
			if (imports.TryGetValue(import, out var value))
				return value;
			return "";
		}
		set
		{
			if (imports.ContainsKey(import))
				imports[import] = value;
		}
	}
	public IEnumerable<IModuleProvider> Descend(string export)
	{
		var exUnit = library[export];

		if (exUnit is NoValue)
			return [];

		var units = ModuleProviderHelpers.GetSubUnits(exUnit);

		return units.Select(u =>
			new ModuleProvider(export, imports, u));
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
		IUnit unit = new NoValue();
		var processor = Modular.GetModuleProcessor(resourceType);

		if (processor == null)
			unit = GetSurfaceNode(path);
		else
		{
			var import = config["import"] as IUnitRecord;
			Dictionary<string, IUnit> arguments = new(StringComparer.OrdinalIgnoreCase);
			int index = 0;
			var imports = import?.FieldNames ?? [];

			if (import != null)
			{
				foreach (var field in imports)
				{

					var argValue = import[field];
					IUnit value;

					if (argValue is IEmptyUnit && index < path.Length)
					{
						value = new UnitValue(id, path[index].ToString() ?? "");
						index++;
					}
					else if (argValue is IUnitValue argUnit)
					{
						value = argUnit;
					}
					else
					{
						value = argValue;
					}

					arguments.TryAdd(field, value);
				}
			}


			IUnitRecord module = config;

			if (index > 0)
			{
				import = new UnitRecord(id, "Import", arguments, _ => _);
				module = new DelayedRecord(module, import);
			}

			unit = processor.ProcessModule(module, path[index..]);
		}




		return unit;
	}

	private IUnit GetSurfaceNode(UnitPath path)
	{
		IUnit unit;
		string name = path[0];
		string[] args = [];
		if (path.Length > 1)
			args = path[1..];

		unit = library[name];

		if (unit is IDelayedUnit func)
			unit = library[func](args);
		return unit;
	}
}

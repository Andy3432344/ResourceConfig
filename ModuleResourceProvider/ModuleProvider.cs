using CatConfig;

namespace ModuleResourceProvider;

public class ModuleProvider : IModuleProvider
{
	private readonly Dictionary<string, string> imports = new(StringComparer.OrdinalIgnoreCase);
	private readonly IUnit unit;


	public ModuleProvider(string name, Dictionary<string, string> imports, IUnit unit)
	{
		foreach (string import in imports.Keys)
			this.imports.Add(import, imports[import]);
		UnitText = name;
		Imports = imports.Keys.ToArray();
		this.unit = unit;

		Exports = unit switch
		{
			IUnitValue vUnit => [vUnit.Value],
			IUnitArray aUnit => Enumerable.Range(0, aUnit.Elements.Length).Select(i => i.ToString()).ToArray(),
			IUnitRecord rUnit => rUnit.FieldNames,
			_ => [],
		};

		if (unit is IUnitRecord record)
			if (record["import"] is IUnitRecord import)
				foreach (var field in import.FieldNames)
					imports[field] = (import[field] as IUnitValue)?.Value ?? $"No-{field}-Value";

	}

	public string this[string import]
	{
		get => imports.GetValueOrDefault(import, "");
		set
		{
			if (imports.ContainsKey(import))
				imports[import] = value;
		}
	}


	public string UnitText { get; }
	public string[] Imports { get; }
	public string[] Exports { get; }

	public IEnumerable<IModuleProvider> Descend(string export)
	{
		if (Exports.Contains(export, StringComparer.OrdinalIgnoreCase))
			if (unit is IUnitRecord record)
				return ModuleProviderHelpers
				.GetSubUnits(record[export]).Select(u => new ModuleProvider(export, imports, u));
			else if (unit is IUnitArray array && int.TryParse(export, out var index))
				return ModuleProviderHelpers
				.GetSubUnits(array.Elements[index]).Select(u => new ModuleProvider($"({UnitText}[{export}]", imports, u));

		if (unit is IUnitValue uVal)
			return [new ModuleProvider(uVal.Value, imports, new NoValue())];

		return [];
	}
}

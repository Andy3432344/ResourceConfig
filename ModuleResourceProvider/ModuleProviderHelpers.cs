using CatConfig;
using CatConfig.CclUnit;

namespace ModuleResourceProvider;

internal static class ModuleProviderHelpers
{

	public static IEnumerable<IUnit> GetSubUnits(IUnit exUnit)
	{
		return exUnit switch
		{
			IUnitRecord record => record.FieldNames.Select(f => record[f]),
			IUnitArray array => array.Elements,
			_ => [exUnit]
		};
	}


	public static (string Key,string? Value)[] GetImports(IUnitRecord config)
	{
		var import = config["import"] as IUnitRecord;
		if (import == null)
			import = new NoRecord();
		Dictionary<string, string?> imports = new(StringComparer.OrdinalIgnoreCase);
		foreach (var field in import.FieldNames)
		{
			string? value = import[field] switch
			{
				IUnitValue imported => imported.Value,
				IEmptyUnit => null,
				NoValue => $"No-{field}-Value",
				_ => ""
			};


			imports.TryAdd(field, value);
		}

		return imports.Select(i=>(i.Key,i.Value)).ToArray();
	}

	public static string[] GetExports(IUnitRecord config)
	{
		var exports = config["exports"] as IUnitArray;
		if (exports == null)
			exports = new NoArray();

		var values = exports.Elements.Select(e => e is UnitValue u ? u.Value : "").ToArray();
		return values;
	}

}
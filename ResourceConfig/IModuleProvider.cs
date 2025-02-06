using CatConfig;

namespace ModuleResourceProvider;

public interface IModuleProvider
{
	string UnitText { get; }
	string[] Imports { get; }
	string[] Exports { get; }
	string this[string import] { get; set; }
	IEnumerable<IModuleProvider> Descend(string export);
}

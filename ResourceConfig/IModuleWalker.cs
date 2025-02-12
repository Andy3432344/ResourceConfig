using CatConfig;

namespace ModuleResourceProvider;

public interface IModuleWalker
{
	string UnitText { get; }
	string[] Imports { get; }
	string[] Exports { get; }
	string this[string import] { get; set; }
	IEnumerable<IModuleWalker> Descend(string export);
}

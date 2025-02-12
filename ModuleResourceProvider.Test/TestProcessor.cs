using CatConfig;
using CatConfig.CclUnit;

namespace ModuleResourceProvider.Test;

public class TestProcessor : IDelayedProcessor
{
	public TestProcessor()
	{
		Constructor.RegisterProcessor(this);
	}
	public string Name => "GetFileName";
	public string ProtocolSchema => "test";

	public IUnit ResolveDelayedUnit(int id, string name, UnitPath path)
	{
		return new UnitValue(id, Path.GetFileNameWithoutExtension(path));
	}
}

